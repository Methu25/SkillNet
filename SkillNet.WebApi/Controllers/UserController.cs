using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Administrator")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IAuditLogService _auditLogService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IAuthenticationService _authenticationService;

        public UserController(
            IConfiguration configuration,
            IAuditLogService auditLogService,
            IPasswordHashService passwordHashService,
            IAuthenticationService authenticationService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _auditLogService = auditLogService;
            _passwordHashService = passwordHashService;
            _authenticationService = authenticationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest(new { message = "Email and password are required." });
            var passwordError = _authenticationService.ValidatePasswordPolicy(user.PasswordHash);
            if (passwordError != null) return BadRequest(new { message = passwordError });

            var names = SplitName(user.Username);
            const string query = @"
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Status, FailedLoginAttempts, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.UserID
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Status, 0, GETDATE(), GETDATE())";

            int newUserId = 0;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using var transaction = con.BeginTransaction();
                try
                {
                    using var cmd = new SqlCommand(query, con, transaction);
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim());
                    string hashedPwd = _passwordHashService.HashPassword(user.PasswordHash);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPwd);
                    cmd.Parameters.AddWithValue("@FirstName", names.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", names.LastName);
                    cmd.Parameters.AddWithValue("@Status", user.IsActive ? "Active" : "Inactive");
                    newUserId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    using var roleCmd = new SqlCommand("INSERT INTO UserRole (UserID, RoleID) VALUES (@UserId, @RoleId)", con, transaction);
                    roleCmd.Parameters.AddWithValue("@UserId", newUserId);
                    roleCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    await roleCmd.ExecuteNonQueryAsync();

                    if (user.OrganizationId.HasValue)
                    {
                        using var orgCmd = new SqlCommand(@"
                            IF EXISTS (SELECT 1 FROM RecruiterProfile WHERE UserId = @UserId)
                                UPDATE RecruiterProfile SET OrganizationId = @OrgId, UpdatedAt = GETDATE() WHERE UserId = @UserId;
                            ELSE
                                INSERT INTO RecruiterProfile (UserId, OrganizationId, CreatedAt, UpdatedAt) VALUES (@UserId, @OrgId, GETDATE(), GETDATE());", con, transaction);
                        orgCmd.Parameters.AddWithValue("@UserId", newUserId);
                        orgCmd.Parameters.AddWithValue("@OrgId", user.OrganizationId.Value);
                        await orgCmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch (SqlException ex) when (ex.Number is 2601 or 2627)
                {
                    transaction.Rollback();
                    return BadRequest(new { message = "A user with this email already exists." });
                }
            }

            await _auditLogService.LogActionAsync("Create User", "Users", newUserId, null, user.Email);
            return Ok(new { message = "User created successfully!" });
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    const string query = @"
                        SELECT u.UserID, u.FirstName, u.LastName, u.Email, u.Status, u.CreatedAt,
                               (SELECT TOP 1 rp.OrganizationId FROM RecruiterProfile rp WHERE rp.UserId = u.UserID) AS OrganizationId,
                               ISNULL((SELECT TOP 1 ur.RoleID FROM UserRole ur WHERE ur.UserID = u.UserID), 0) AS RoleId,
                               ISNULL((SELECT TOP 1 r.RoleName FROM Roles r JOIN UserRole ur ON ur.RoleID = r.RoleID WHERE ur.UserID = u.UserID), '') AS Roles
                        FROM Users u
                        ORDER BY u.CreatedAt DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var users = new List<object>();
                            while (reader.Read())
                            {
                                string firstName = reader["FirstName"] == DBNull.Value ? "" : reader["FirstName"].ToString() ?? "";
                                string lastName = reader["LastName"] == DBNull.Value ? "" : reader["LastName"].ToString() ?? "";
                                string email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString() ?? "";
                                string status = reader["Status"] == DBNull.Value ? "Active" : reader["Status"].ToString() ?? "Active";
                                string username = $"{firstName} {lastName}".Trim();
                                if (string.IsNullOrWhiteSpace(username)) username = email;

                                users.Add(new
                                {
                                    UserId = Convert.ToInt32(reader["UserID"]),
                                    Username = username,
                                    Email = email,
                                    RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]),
                                    Roles = reader["Roles"] == DBNull.Value ? "" : reader["Roles"].ToString(),
                                    IsActive = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase),
                                    OrganizationId = reader["OrganizationId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["OrganizationId"]),
                                    DepartmentId = (int?)null,
                                    CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                            return Ok(users);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching users: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            var names = SplitName(user.Username);
            const string query = "UPDATE Users SET FirstName=@FirstName, LastName=@LastName, Email=@Email, UpdatedAt=GETDATE() WHERE UserID=@UserId";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using var transaction = con.BeginTransaction();
                using (SqlCommand cmd = new SqlCommand(query, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@FirstName", names.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", names.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email.Trim());
                    if (await cmd.ExecuteNonQueryAsync() == 0) return NotFound(new { message = "User not found." });
                }
                using var roleCmd = new SqlCommand("DELETE FROM UserRole WHERE UserID=@UserId; INSERT INTO UserRole(UserID,RoleID) VALUES(@UserId,@RoleId);", con, transaction);
                roleCmd.Parameters.AddWithValue("@UserId", id);
                roleCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                await roleCmd.ExecuteNonQueryAsync();

                if (user.OrganizationId.HasValue)
                {
                    using var orgCmd = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM RecruiterProfile WHERE UserId = @UserId)
                            UPDATE RecruiterProfile SET OrganizationId = @OrgId, UpdatedAt = GETDATE() WHERE UserId = @UserId;
                        ELSE
                            INSERT INTO RecruiterProfile (UserId, OrganizationId, CreatedAt, UpdatedAt) VALUES (@UserId, @OrgId, GETDATE(), GETDATE());", con, transaction);
                    orgCmd.Parameters.AddWithValue("@UserId", id);
                    orgCmd.Parameters.AddWithValue("@OrgId", user.OrganizationId.Value);
                    await orgCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }

            await _auditLogService.LogActionAsync("Update User", "Users", id, null, user.Email);
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            string query = "UPDATE Users SET Status=CASE WHEN Status='Active' THEN 'Inactive' ELSE 'Active' END, UpdatedAt=GETDATE() WHERE UserID=@UserId";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            await _auditLogService.LogActionAsync("Toggle User Status", "Users", id, null, null);
            return Ok(new { message = "User status toggled successfully!" });
        }

        public class ResetPasswordRequest { public string NewPassword { get; set; } = string.Empty; }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest req)
        {
            if (string.IsNullOrEmpty(req.NewPassword)) return BadRequest("Password cannot be empty");
            var passwordError = _authenticationService.ValidatePasswordPolicy(req.NewPassword);
            if (passwordError != null) return BadRequest(new { message = passwordError });

            string hashedPwd = _passwordHashService.HashPassword(req.NewPassword);
            string query = "UPDATE Users SET PasswordHash=@Pwd, FailedLoginAttempts=0, LockoutEnd=NULL, UpdatedAt=GETDATE() WHERE UserID=@UserId";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@Pwd", hashedPwd);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            await _auditLogService.LogActionAsync("Reset Password", "Users", id, null, null);
            return Ok(new { message = "Password reset successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            string query = "DELETE FROM Users WHERE UserID = @UserId";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            await _auditLogService.LogActionAsync("Delete User", "Users", id, null, null);
            return Ok(new { message = "User deleted successfully!" });
        }

        private static (string FirstName, string LastName) SplitName(string? username)
        {
            var parts = (username ?? string.Empty).Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => ("User", "Account"),
                1 => (parts[0], "Account"),
                _ => (parts[0], parts[1])
            };
        }
    }
}
