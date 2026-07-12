using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;
using SkillNet.Server.Services;
using SkillNet.Server.Utilities;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Temporarily disabled for frontend testing
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
            string query = @"INSERT INTO Users (Username, Email, PasswordHash, RoleId, IsActive, OrganizationId, DepartmentId, CreatedAt) 
                             VALUES (@Username, @Email, @PasswordHash, @RoleId, @IsActive, @OrganizationId, @DepartmentId, @CreatedAt)";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);

                    // Note: In a real app, always hash passwords before saving! 
                    string hashedPwd = PasswordHasher.HashPassword(user.PasswordHash ?? "Default@123");
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPwd);

                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    con.Open();
                    try 
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex) when (ex.Number == 2627)
                    {
                        return BadRequest(new { message = "A user with this email already exists." });
                    }
                }
            }
            
            await _auditLogService.LogActionAsync("Create User", "Users", null, null, user.Email);
            
            return Ok(new { message = "User created successfully!" });
        }
        [HttpGet]
        public IActionResult GetUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT UserId, Username, Email, RoleId, IsActive, OrganizationId, DepartmentId, CreatedAt FROM Users";
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
                                Email = reader["Email"].ToString(),
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
            string query = "UPDATE Users SET Username = @Username, Email = @Email, RoleId = @RoleId, OrganizationId = @OrganizationId, DepartmentId = @DepartmentId WHERE UserId = @UserId";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@Username", user.Username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@OrganizationId", user.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DepartmentId", user.DepartmentId ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            
            await _auditLogService.LogActionAsync("Update User", "Users", id, null, user.Email);
            
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            string query = "UPDATE Users SET IsActive = IsActive ^ 1 WHERE UserId = @UserId";
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
            string query = "UPDATE Users SET PasswordHash = @Pwd WHERE UserId = @UserId";
            
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
            string query = "DELETE FROM Users WHERE UserId = @UserId";
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