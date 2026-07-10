using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly string _connectionString;

        public DashboardController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // GET: api/dashboard/statistics
        [HttpGet("statistics")]
        public IActionResult GetDashboardStatistics()
        {
            int totalUsers = 0;
            int totalOrgs = 0;
            int totalDepts = 0;
            List<string> recentActions = new List<string>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // 1. Get Total Users
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", con))
                {
                    totalUsers = (int)cmd.ExecuteScalar();
                }

                // 2. Get Total Organizations
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Organization", con))
                {
                    totalOrgs = (int)cmd.ExecuteScalar();
                }

                // 3. Get Total Departments
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Department", con))
                {
                    totalDepts = (int)cmd.ExecuteScalar();
                }

                // 4. Get 5 Most Recent Audit Log Actions
                string recentQuery = "SELECT TOP 5 Action FROM AuditLog ORDER BY Timestamp DESC";
                using (SqlCommand cmd = new SqlCommand(recentQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recentActions.Add(reader["Action"].ToString() ?? "");
                    }
                }
            }

            // Return all stats combined for the React Admin Dashboard
            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalOrganizations = totalOrgs,
                TotalDepartments = totalDepts,
                RecentActivities = recentActions
            });
        }
    }
}