using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly string _connectionString;

        public SystemConfigurationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // GET: api/systemconfiguration
        [HttpGet]
        public IActionResult GetConfigurations()
        {
            List<SystemConfiguration> configs = new List<SystemConfiguration>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT ConfigKey, ConfigValue, Description, UpdatedAt FROM SystemConfiguration";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            configs.Add(new SystemConfiguration
                            {
                                ConfigKey = reader["ConfigKey"].ToString() ?? "",
                                ConfigValue = reader["ConfigValue"].ToString() ?? "",
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                            });
                        }
                    }
                }
            }
            return Ok(configs);
        }
    }
}