using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            string query = @"INSERT INTO Users (Username, Email, PasswordHash, RoleId, CreatedAt) 
                             VALUES (@Username, @Email, @PasswordHash, @RoleId, @CreatedAt)";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);

                    // Note: In a real app, always hash passwords before saving! 
                    // We are saving it directly here just to get the API working first.
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "User created successfully!" });
        }
        [HttpGet]
        public IActionResult GetUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT UserId, Username, Email, RoleId, CreatedAt FROM Users";
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
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(users);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            string query = "UPDATE Users SET Username = @Username, Email = @Email, RoleId = @RoleId WHERE UserId = @UserId";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@Username", user.Username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
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
            return Ok(new { message = "User deleted successfully!" });
        }
    }
}