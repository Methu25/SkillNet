using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrganizationController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IAuditLogService _auditLogService;

        public OrganizationController(IConfiguration configuration, IAuditLogService auditLogService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _auditLogService = auditLogService;
        }

        // GET: api/organization
        [HttpGet]
        public IActionResult GetOrganizations()
        {
            List<Organization> orgs = new List<Organization>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT OrganizationId, OrganizationName, Industry, Website, Logo, Address, CreatedAt FROM Organization";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orgs.Add(new Organization
                            {
                                OrganizationId = Convert.ToInt32(reader["OrganizationId"]),
                                OrganizationName = reader["OrganizationName"].ToString() ?? "",
                                Industry = reader["Industry"] != DBNull.Value ? reader["Industry"].ToString() : null,
                                Website = reader["Website"] != DBNull.Value ? reader["Website"].ToString() : null,
                                Logo = reader["Logo"] != DBNull.Value ? reader["Logo"].ToString() : null,
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(orgs);
        }

        // POST: api/organization
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] Organization org)
        {
            string query = @"INSERT INTO Organization (OrganizationName, Industry, Website, Logo, Address, CreatedAt) 
                     VALUES (@Name, @Industry, @Website, @Logo, @Address, @CreatedAt)";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", org.OrganizationName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Industry", org.Industry ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", org.Website ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Logo", org.Logo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", org.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex) when (ex.Number == 2627)
                    {
                        return BadRequest(new { message = "An organization with this name already exists." });
                    }
                }
            }

            await _auditLogService.LogActionAsync("Create Organization", "Organization", null, null, org.OrganizationName);
            return Ok(new { message = "Organization created successfully!" });
        }

        // PUT: api/organization/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] Organization org)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Organization 
                                 SET OrganizationName = @Name, Industry = @Industry, Website = @Website, Logo = @Logo, Address = @Address
                                 WHERE OrganizationId = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", org.OrganizationName);
                    cmd.Parameters.AddWithValue("@Industry", (object?)org.Industry ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", (object?)org.Website ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Logo", (object?)org.Logo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object?)org.Address ?? DBNull.Value);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0) return NotFound(new { message = "Organization not found" });
                }
            }

            await _auditLogService.LogActionAsync("Update Organization", "Organization", id, null, org.OrganizationName);
            return Ok(new { message = "Organization updated successfully" });
        }

        // DELETE: api/organization/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            try
            {
                string query = "DELETE FROM Organization WHERE OrganizationId = @Id";
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                await _auditLogService.LogActionAsync("Delete Organization", "Organization", id, null, null);
                return Ok(new { message = "Organization deleted successfully!" });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return BadRequest(new { message = "Cannot delete: Organization has linked departments or users." });
            }
        }
    }
}
