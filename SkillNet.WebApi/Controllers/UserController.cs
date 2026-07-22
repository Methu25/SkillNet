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
                INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Status, OrganizationId, DepartmentId, FailedLoginAttempts, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.UserID
                VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Status, @OrganizationId, @DepartmentId, 0, GETDATE(), GETDATE())";

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
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);
                    newUserId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    using var roleCmd = new SqlCommand("INSERT INTO UserRole (UserID, RoleID) VALUES (@UserId, @RoleId)", con, transaction);
                    roleCmd.Parameters.AddWithValue("@UserId", newUserId);
                    roleCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    await roleCmd.ExecuteNonQueryAsync();
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
                        SELECT u.UserID, u.FirstName, u.LastName, u.Email, u.Status, u.OrganizationId, u.DepartmentId, u.CreatedAt,
                               ISNULL(MIN(ur.RoleID), 0) RoleId, ISNULL(STRING_AGG(r.RoleName, ', '), '') Roles
                        FROM Users u
                        LEFT JOIN UserRole ur ON ur.UserID=u.UserID
                        LEFT JOIN Roles r ON r.RoleID=ur.RoleID
                        GROUP BY u.UserID,u.FirstName,u.LastName,u.Email,u.Status,u.OrganizationId,u.DepartmentId,u.CreatedAt
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
                                    DepartmentId = reader["DepartmentId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["DepartmentId"]),
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
            const string query = "UPDATE Users SET FirstName=@FirstName, LastName=@LastName, Email=@Email, OrganizationId=@OrganizationId, DepartmentId=@DepartmentId, UpdatedAt=GETDATE() WHERE UserID=@UserId";
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
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);
                    if (await cmd.ExecuteNonQueryAsync() == 0) return NotFound(new { message = "User not found." });
                }
                using var roleCmd = new SqlCommand("DELETE FROM UserRole WHERE UserID=@UserId; INSERT INTO UserRole(UserID,RoleID) VALUES(@UserId,@RoleId);", con, transaction);
                roleCmd.Parameters.AddWithValue("@UserId", id);
                roleCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                await roleCmd.ExecuteNonQueryAsync();
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
