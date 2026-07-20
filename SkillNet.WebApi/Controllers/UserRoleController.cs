using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly string _connectionString;

        public UserRoleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // GET: api/userrole
        [HttpGet]
        public IActionResult GetRoles()
        {
            List<UserRole> roles = new List<UserRole>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT RoleId, RoleName, Description, CreatedAt FROM UserRole";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new UserRole
                            {
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                RoleName = reader["RoleName"].ToString() ?? "",
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(roles);
        }

        // POST: api/userrole
        [HttpPost]
        public IActionResult CreateRole([FromBody] UserRole role)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO UserRole (RoleName, Description) 
                                 VALUES (@Name, @Desc)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", role.RoleName);
                    cmd.Parameters.AddWithValue("@Desc", (object?)role.Description ?? DBNull.Value);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "User Role created successfully" });
        }
    }
}
