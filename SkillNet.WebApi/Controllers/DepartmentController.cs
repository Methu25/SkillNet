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
    public class DepartmentController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IAuditLogService _auditLogService;

        public DepartmentController(IConfiguration configuration, IAuditLogService auditLogService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _auditLogService = auditLogService;
        }

        // GET: api/department
        [HttpGet]
        public IActionResult GetDepartments()
        {
            List<Department> depts = new List<Department>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
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
        public async Task<IActionResult> CreateDepartment([FromBody] Department dept)
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

            await _auditLogService.LogActionAsync("Create Department", "Department", null, null, dept.DepartmentName);
            return Ok(new { message = "Department created successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] Department dept)
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

            await _auditLogService.LogActionAsync("Update Department", "Department", id, null, dept.DepartmentName);
            return Ok(new { message = "Department updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
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

                await _auditLogService.LogActionAsync("Delete Department", "Department", id, null, null);
                return Ok(new { message = "Department deleted successfully!" });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest(new { message = "Cannot delete: Department has linked employees." });
            }
        }
    }
}
