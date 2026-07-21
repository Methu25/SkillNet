using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SkillNet.Application.Services
{
    public interface ISystemConfigurationService
    {
        string GetSetting(string key, string defaultValue);
        bool GetBoolSetting(string key, bool defaultValue);
        int GetIntSetting(string key, int defaultValue);
    }

    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly string _connectionString;

        public SystemConfigurationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public string GetSetting(string key, string defaultValue)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT [Value] FROM SystemConfiguration WHERE [Key] = @Key";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Key", key);
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString() ?? defaultValue;
                        }
                    }
                }
            }
            catch
            {
                // Fallback to default safely if DB is unreachable or missing table
            }
            return defaultValue;
        }

        public bool GetBoolSetting(string key, bool defaultValue)
        {
            string value = GetSetting(key, defaultValue.ToString());
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        public int GetIntSetting(string key, int defaultValue)
        {
            string value = GetSetting(key, defaultValue.ToString());
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
