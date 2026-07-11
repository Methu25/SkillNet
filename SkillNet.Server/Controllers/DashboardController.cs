using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Temporarily disabled for frontend testing
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
            int totalCandidates = 0;
            int totalRecruiters = 0;
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

                // 1.5 Get Total Candidates & Recruiters
                string rolesQuery = @"SELECT r.RoleName, COUNT(u.UserId) as Count 
                                      FROM Users u 
                                      JOIN UserRole r ON u.RoleId = r.RoleId 
                                      GROUP BY r.RoleName";
                using (SqlCommand cmd = new SqlCommand(rolesQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string roleName = reader["RoleName"].ToString()!;
                        int count = Convert.ToInt32(reader["Count"]);
                        if (roleName == "Candidate") totalCandidates = count;
                        if (roleName == "Recruiter") totalRecruiters = count;
                    }
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
                TotalCandidates = totalCandidates,
                TotalRecruiters = totalRecruiters,
                TotalOrganizations = totalOrgs,
                TotalDepartments = totalDepts,
                RecentActivities = recentActions
            });
        }
    }
}