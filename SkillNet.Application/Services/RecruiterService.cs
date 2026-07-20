using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class RecruiterService : IRecruiterService
    {
        private readonly string _connectionString;

        public RecruiterService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<RecruiterProfileDto?> GetProfileAsync(int userId)
        {
            const string query = @"
                SELECT rp.*, o.OrganizationName
                FROM RecruiterProfile rp
                LEFT JOIN Organization o ON rp.OrganizationId = o.OrganizationId
                WHERE rp.UserId = @UserId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RecruiterProfileDto
                {
                    UserId = userId,
                    Headline = reader["Headline"] == DBNull.Value ? null : reader["Headline"].ToString(),
                    Bio = reader["Bio"] == DBNull.Value ? null : reader["Bio"].ToString(),
                    LinkedInUrl = reader["LinkedInUrl"] == DBNull.Value ? null : reader["LinkedInUrl"].ToString(),
                    ExperienceYears = reader["ExperienceYears"] == DBNull.Value ? null : (int)reader["ExperienceYears"],
                    OrganizationId = reader["OrganizationId"] == DBNull.Value ? null : (int)reader["OrganizationId"],
                    OrganizationName = reader["OrganizationName"] == DBNull.Value ? null : reader["OrganizationName"].ToString()
                };
            }
            return null;
        }

        public async Task<RecruiterProfileDto> UpsertProfileAsync(int userId, RecruiterProfileDto dto)
        {
            const string query = @"
                IF EXISTS (SELECT 1 FROM RecruiterProfile WHERE UserId = @UserId)
                    UPDATE RecruiterProfile
                    SET Headline = @Headline, Bio = @Bio, LinkedInUrl = @LinkedInUrl,
                        ExperienceYears = @ExperienceYears, OrganizationId = @OrganizationId,
                        UpdatedAt = GETDATE()
                    WHERE UserId = @UserId
                ELSE
                    INSERT INTO RecruiterProfile (UserId, Headline, Bio, LinkedInUrl, ExperienceYears, OrganizationId)
                    VALUES (@UserId, @Headline, @Bio, @LinkedInUrl, @ExperienceYears, @OrganizationId)";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Headline", dto.Headline ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Bio", dto.Bio ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LinkedInUrl", dto.LinkedInUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceYears", dto.ExperienceYears ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@OrganizationId", dto.OrganizationId ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            return (await GetProfileAsync(userId))!;
        }

        public async Task<int?> GetRecruiterProfileIdAsync(int userId)
        {
            const string query = "SELECT RecruiterProfileId FROM RecruiterProfile WHERE UserId = @UserId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            var result = await cmd.ExecuteScalarAsync();

            return result == null || result == DBNull.Value ? null : (int)result;
        }

        public async Task<RecruiterDashboardDto> GetDashboardStatsAsync(int userId)
        {
            var recruiterProfileId = await GetRecruiterProfileIdAsync(userId);
            if (!recruiterProfileId.HasValue)
                return new RecruiterDashboardDto();

            const string query = @"
                SELECT
                    COUNT(*) AS TotalJobs,
                    SUM(CASE WHEN Status = 'Published' THEN 1 ELSE 0 END) AS PublishedJobs,
                    SUM(CASE WHEN Status = 'Draft' THEN 1 ELSE 0 END) AS DraftJobs,
                    SUM(CASE WHEN Status = 'Closed' THEN 1 ELSE 0 END) AS ClosedJobs
                FROM JobPost
                WHERE RecruiterId = @RecruiterId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RecruiterId", recruiterProfileId.Value);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RecruiterDashboardDto
                {
                    TotalJobs = reader["TotalJobs"] == DBNull.Value ? 0 : (int)reader["TotalJobs"],
                    PublishedJobs = reader["PublishedJobs"] == DBNull.Value ? 0 : (int)reader["PublishedJobs"],
                    DraftJobs = reader["DraftJobs"] == DBNull.Value ? 0 : (int)reader["DraftJobs"],
                    ClosedJobs = reader["ClosedJobs"] == DBNull.Value ? 0 : (int)reader["ClosedJobs"],
                    TotalApplicationsReceived = 0
                };
            }
            return new RecruiterDashboardDto();
        }
    }
}
