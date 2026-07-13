using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;

namespace SkillNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Temporarily disabled for frontend testing
    public class SystemConfigurationController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IAuditLogService _auditLogService;

        public SystemConfigurationController(IConfiguration configuration, IAuditLogService auditLogService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public IActionResult GetConfigs()
        {
            List<SystemConfiguration> configs = new List<SystemConfiguration>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
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
        public async Task<IActionResult> UpdateConfigs([FromBody] List<SystemConfiguration> updatedConfigs)
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
                    await _auditLogService.LogActionAsync("Update System Setting", "SystemConfiguration", null, null, $"{config.Key}={config.Value}");
                }
            }
            return Ok(new { message = "Settings saved successfully!" });
        }
    }
}
