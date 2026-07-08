using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly string _connectionString;

        public OrganizationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
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
        public IActionResult CreateOrganization([FromBody] Organization org)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Using parameters to prevent SQL injection!
                string query = @"INSERT INTO Organization (OrganizationName, Industry, Website, Logo, Address) 
                                 VALUES (@Name, @Industry, @Website, @Logo, @Address)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", org.OrganizationName);
                    cmd.Parameters.AddWithValue("@Industry", (object?)org.Industry ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Website", (object?)org.Website ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Logo", (object?)org.Logo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object?)org.Address ?? DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Organization created successfully" });
        }
    }
}