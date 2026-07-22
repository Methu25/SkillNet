using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
                string query = "SELECT RoleID as RoleId, RoleName, NULL as Description, GETDATE() as CreatedAt FROM Roles ORDER BY RoleName";
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
                                Description = null,
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
                string query = "INSERT INTO Roles (RoleName) VALUES (@Name)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", role.RoleName);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "User Role created successfully" });
        }
    }
}
