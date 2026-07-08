using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly string _connectionString;

        public DepartmentController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // GET: api/department
        [HttpGet]
        public IActionResult GetDepartments()
        {
            List<Department> departments = new List<Department>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepartmentId, OrganizationId, DepartmentName, Description, CreatedAt FROM Department";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new Department
                            {
                                DepartmentId = Convert.ToInt32(reader["DepartmentId"]),
                                OrganizationId = Convert.ToInt32(reader["OrganizationId"]),
                                DepartmentName = reader["DepartmentName"].ToString() ?? "",
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(departments);
        }

        // POST: api/department
        [HttpPost]
        public IActionResult CreateDepartment([FromBody] Department dept)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Department (OrganizationId, DepartmentName, Description) 
                                 VALUES (@OrgId, @Name, @Desc)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@OrgId", dept.OrganizationId);
                    cmd.Parameters.AddWithValue("@Name", dept.DepartmentName);
                    cmd.Parameters.AddWithValue("@Desc", (object?)dept.Description ?? DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Department created successfully" });
        }
    }
}