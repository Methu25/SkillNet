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

        [HttpGet]
        public IActionResult GetConfigs()
        {
            List<SystemConfiguration> configs = new List<SystemConfiguration>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Using brackets [ ] because Key and Value are reserved SQL words
                string query = "SELECT [Key], [Value], Description FROM SystemConfiguration";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            configs.Add(new SystemConfiguration
                            {
                                Key = reader["Key"].ToString() ?? "",
                                Value = reader["Value"].ToString() ?? "",
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null
                            });
                        }
                    }
                }
            }
            return Ok(configs);
        }
        [HttpPut]
        public IActionResult UpdateConfigs([FromBody] List<SystemConfiguration> updatedConfigs)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                foreach (var config in updatedConfigs)
                {
                    string query = "UPDATE SystemConfiguration SET [Value] = @Value WHERE [Key] = @Key";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Value", config.Value);
                        cmd.Parameters.AddWithValue("@Key", config.Key);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return Ok(new { message = "Settings saved successfully!" });
        }
    }
}