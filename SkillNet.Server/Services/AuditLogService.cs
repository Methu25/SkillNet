using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace SkillNet.Server.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _connectionString;

        public AuditLogService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task LogActionAsync(string action, string? entity = null, int? entityId = null, string? oldValue = null, string? newValue = null)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            int? userId = null;
            
            // Extract the user ID from the claims we add in AuthController
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                // Fallback to UserId = 1 (System Admin) for testing when not authenticated
                userId = 1;
            }

            // Extract the user's IP Address
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO AuditLog (UserId, Action, Entity, EntityId, OldValue, NewValue, Timestamp, IPAddress) 
                                 VALUES (@UserId, @Action, @Entity, @EntityId, @OldValue, @NewValue, @Timestamp, @IPAddress)";
                                 
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Entity", entity ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntityId", entityId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OldValue", oldValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewValue", newValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? (object)DBNull.Value);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
