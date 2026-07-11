using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SkillNet.Server.models;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Temporarily disabled for frontend testing
    public class AuditLogController : ControllerBase
    {
        private readonly string _connectionString;

        public AuditLogController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // GET: api/auditlog
        // Includes coursework requirement: filtering options by user, date, and action
        [HttpGet]
        public IActionResult GetAuditLogs(
            [FromQuery] int? userId,
            [FromQuery] string? action,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            List<AuditLog> logs = new List<AuditLog>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Base query with 1=1 to easily append dynamic AND conditions
                string query = "SELECT AuditLogId, UserId, Action, Entity, EntityId, OldValue, NewValue, Timestamp, IPAddress FROM AuditLog WHERE 1=1";

                if (userId.HasValue) query += " AND UserId = @UserId";
                if (!string.IsNullOrEmpty(action)) query += " AND Action LIKE @Action";
                if (startDate.HasValue) query += " AND Timestamp >= @StartDate";
                if (endDate.HasValue) query += " AND Timestamp <= @EndDate";

                query += " ORDER BY Timestamp DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (userId.HasValue) cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    if (!string.IsNullOrEmpty(action)) cmd.Parameters.AddWithValue("@Action", "%" + action + "%");
                    if (startDate.HasValue) cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                    if (endDate.HasValue) cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AuditLog
                            {
                                AuditLogId = Convert.ToInt32(reader["AuditLogId"]),
                                UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt32(reader["UserId"]) : null,
                                Action = reader["Action"].ToString() ?? "",
                                Entity = reader["Entity"] != DBNull.Value ? reader["Entity"].ToString() : null,
                                EntityId = reader["EntityId"] != DBNull.Value ? Convert.ToInt32(reader["EntityId"]) : null,
                                OldValue = reader["OldValue"] != DBNull.Value ? reader["OldValue"].ToString() : null,
                                NewValue = reader["NewValue"] != DBNull.Value ? reader["NewValue"].ToString() : null,
                                Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                                IPAddress = reader["IPAddress"] != DBNull.Value ? reader["IPAddress"].ToString() : null
                            });
                        }
                    }
                }
            }
            return Ok(logs);
        }
    }
}