using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Application.Services;
using SkillNet.Application.Utilities;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IAuditLogService _auditLogService;

        public UserController(IConfiguration configuration, IAuditLogService auditLogService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _auditLogService = auditLogService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            int newUserId = 0;
            string query = @"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Status, OrganizationId, DepartmentId, CreatedAt) 
                             OUTPUT INSERTED.UserID
                             VALUES (@FirstName, '', @Email, @PasswordHash, @Status, @OrganizationId, @DepartmentId, @CreatedAt)";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.Username ?? "User");
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    
                    string hashedPwd = PasswordHasher.HashPassword(user.PasswordHash ?? "Default@123");
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPwd);
                    
                    cmd.Parameters.AddWithValue("@Status", user.IsActive ? "Active" : "Inactive");
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    con.Open();
                    try
                    {
                        newUserId = (int)cmd.ExecuteScalar();
                    }
                    catch (SqlException ex) when (ex.Number == 2627)
                    {
                        return BadRequest(new { message = "A user with this email already exists." });
                    }
                }

                if (newUserId > 0)
                {
                    string roleQuery = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserId, @RoleId)";
                    using (SqlCommand roleCmd = new SqlCommand(roleQuery, con))
                    {
                        roleCmd.Parameters.AddWithValue("@UserId", newUserId);
                        roleCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                        roleCmd.ExecuteNonQuery();
                    }
                }
            }

            await _auditLogService.LogActionAsync("Create User", "Users", newUserId, null, user.Email);
            return Ok(new { message = "User created successfully!" });
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
                                    u.UserID as UserId, 
                                    u.FirstName as Username, 
                                    u.Email, 
                                    ISNULL((SELECT TOP 1 RoleID FROM UserRole WHERE UserID = u.UserID), 1) as RoleId, 
                                    CAST(CASE WHEN u.Status = 'Active' THEN 1 ELSE 0 END AS BIT) as IsActive, 
                                    u.OrganizationId, 
                                    u.DepartmentId, 
                                    u.CreatedAt 
                                 FROM Users u";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Username = reader["Username"].ToString(),
                                Email = reader["Email"].ToString()!,
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                OrganizationId = reader["OrganizationId"] != DBNull.Value ? Convert.ToInt32(reader["OrganizationId"]) : null,
                                DepartmentId = reader["DepartmentId"] != DBNull.Value ? Convert.ToInt32(reader["DepartmentId"]) : null,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Users SET FirstName = @FirstName, Email = @Email, OrganizationId = @OrganizationId, DepartmentId = @DepartmentId WHERE UserID = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@FirstName", user.Username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                string deleteRoles = "DELETE FROM UserRole WHERE UserID = @UserId";
                using (SqlCommand delCmd = new SqlCommand(deleteRoles, con))
                {
                    delCmd.Parameters.AddWithValue("@UserId", id);
                    delCmd.ExecuteNonQuery();
                }

                string insertRole = "INSERT INTO UserRole (UserID, RoleID) VALUES (@UserId, @RoleId)";
                using (SqlCommand insCmd = new SqlCommand(insertRole, con))
                {
                    insCmd.Parameters.AddWithValue("@UserId", id);
                    insCmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    insCmd.ExecuteNonQuery();
                }
            }

            await _auditLogService.LogActionAsync("Update User", "Users", id, null, user.Email);
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            string query = "UPDATE Users SET Status = CASE WHEN Status = 'Active' THEN 'Inactive' ELSE 'Active' END WHERE UserID = @UserId";
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

            string hashedPwd = PasswordHasher.HashPassword(req.NewPassword);
            string query = "UPDATE Users SET PasswordHash = @Pwd WHERE UserID = @UserId";

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
    }
}
