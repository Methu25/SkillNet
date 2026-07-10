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
            List<Department> depts = new List<Department>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Updated to match your exact columns
                string query = "SELECT DepartmentId, OrganizationId, DepartmentName FROM Department";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            depts.Add(new Department
                            {
                                DepartmentId = Convert.ToInt32(reader["DepartmentId"]),
                                OrganizationId = Convert.ToInt32(reader["OrganizationId"]),
                                DepartmentName = reader["DepartmentName"].ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return Ok(depts);
        }

        // POST: api/department
        [HttpPost]
        public IActionResult CreateDepartment([FromBody] Department dept)
        {
            string query = @"INSERT INTO Department (OrganizationId, DepartmentName) 
                             VALUES (@OrganizationId, @DepartmentName)";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@OrganizationId", dept.OrganizationId);
                    cmd.Parameters.AddWithValue("@DepartmentName", dept.DepartmentName ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Department created successfully!" });
        }
        [HttpPut("{id}")]
        public IActionResult UpdateDepartment(int id, [FromBody] Department dept)
        {
            string query = "UPDATE Department SET DepartmentName = @Name, OrganizationId = @OrgId WHERE DepartmentId = @Id";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", dept.DepartmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrgId", dept.OrganizationId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Department updated successfully!" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDepartment(int id)
        {
            try
            {
                string query = "DELETE FROM Department WHERE DepartmentId = @Id";
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return Ok(new { message = "Department deleted successfully!" });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest(new { message = "Cannot delete: Department has linked employees." });
            }
        }
    }
}