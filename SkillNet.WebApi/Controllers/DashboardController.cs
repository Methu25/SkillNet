using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace SkillNet.WebApi.Controllers
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
            int activeJobs = 0;
            int appsToday = 0;
            int interviewsToday = 0;
            int hiresThisMonth = 0;
            List<string> recentActions = new List<string>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", con))
                    totalUsers = (int)cmd.ExecuteScalar();

                string rolesQuery = @"SELECT r.RoleName, COUNT(ur.UserId) as Count 
                                      FROM UserRole ur 
                                      JOIN Roles r ON ur.RoleId = r.RoleId 
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

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Organization", con))
                    totalOrgs = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Department", con))
                    totalDepts = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM JobPost WHERE Status NOT IN ('Closed', 'Draft')", con))
                    activeJobs = (int)cmd.ExecuteScalar();
                
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM JobApplications WHERE CAST(AppliedDate AS DATE) = CAST(GETDATE() AS DATE)", con))
                    appsToday = (int)cmd.ExecuteScalar();
                
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE)", con))
                    interviewsToday = (int)cmd.ExecuteScalar();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM JobApplications WHERE CurrentStatus = 'Hired' AND MONTH(AppliedDate) = MONTH(GETDATE()) AND YEAR(AppliedDate) = YEAR(GETDATE())", con))
                    hiresThisMonth = (int)cmd.ExecuteScalar();

                string recentQuery = "SELECT TOP 5 Action FROM AuditLog ORDER BY Timestamp DESC";
                using (SqlCommand cmd = new SqlCommand(recentQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        recentActions.Add(reader["Action"].ToString() ?? "");
                }
            }

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalCandidates = totalCandidates,
                TotalRecruiters = totalRecruiters,
                TotalOrganizations = totalOrgs,
                TotalDepartments = totalDepts,
                ActiveJobs = activeJobs,
                ApplicationsToday = appsToday,
                InterviewsToday = interviewsToday,
                HiresThisMonth = hiresThisMonth,
                RecentActivities = recentActions
            });
        }
    }
}
