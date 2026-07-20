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
                        ExperienceYears = @ExperienceYears, UpdatedAt = GETDATE()
                    WHERE UserId = @UserId
                ELSE
                    INSERT INTO RecruiterProfile (UserId, Headline, Bio, LinkedInUrl, ExperienceYears)
                    VALUES (@UserId, @Headline, @Bio, @LinkedInUrl, @ExperienceYears)";

            if (dto.OrganizationId.HasValue)
                throw new ArgumentException(
                    "OrganizationId cannot be assigned through the recruiter profile endpoint.",
                    nameof(dto));

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Headline", dto.Headline ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Bio", dto.Bio ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LinkedInUrl", dto.LinkedInUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceYears", dto.ExperienceYears ?? (object)DBNull.Value);
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

        public async Task<RecruiterOrganizationDto?> GetOrganizationAsync(int userId)
        {
            const string query = @"
                SELECT o.OrganizationId, o.OrganizationName, o.Industry, o.Website,
                       o.Logo, o.Address, o.CreatedAt, o.ApprovalStatus,
                       o.SubmittedAt, o.ReviewedAt, o.RejectionReason
                FROM RecruiterProfile rp
                JOIN Organization o ON o.OrganizationId = rp.OrganizationId
                WHERE rp.UserId = @UserId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapOrganization(reader) : null;
        }

        public async Task<RecruiterOrganizationDto> UpsertOrganizationAsync(
            int userId,
            UpsertRecruiterOrganizationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(request.OrganizationName))
                throw new ArgumentException("Organization name is required.", nameof(request));

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();

            try
            {
                int recruiterProfileId;
                int? organizationId;
                string? approvalStatus;
                const string profileQuery = @"
                    SELECT rp.RecruiterProfileId, rp.OrganizationId, o.ApprovalStatus
                    FROM RecruiterProfile rp
                    LEFT JOIN Organization o ON o.OrganizationId = rp.OrganizationId
                    WHERE rp.UserId = @UserId";

                using (var profileCmd = new SqlCommand(profileQuery, con, transaction))
                {
                    profileCmd.Parameters.AddWithValue("@UserId", userId);
                    using var reader = await profileCmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                        throw new InvalidOperationException("Recruiter profile not yet created.");

                    recruiterProfileId = (int)reader["RecruiterProfileId"];
                    organizationId = reader["OrganizationId"] == DBNull.Value
                        ? null
                        : (int)reader["OrganizationId"];
                    approvalStatus = reader["ApprovalStatus"] == DBNull.Value
                        ? null
                        : reader["ApprovalStatus"].ToString();
                }

                if (organizationId.HasValue)
                {
                    if (approvalStatus is "Pending" or "Approved")
                        throw new InvalidOperationException(
                            $"An organization with {approvalStatus} status cannot be edited.");

                    const string updateQuery = @"
                        UPDATE Organization
                        SET OrganizationName = @OrganizationName, Industry = @Industry,
                            Website = @Website, Logo = @Logo, Address = @Address
                        WHERE OrganizationId = @OrganizationId";
                    using var updateCmd = new SqlCommand(updateQuery, con, transaction);
                    AddOrganizationParameters(updateCmd, request);
                    updateCmd.Parameters.AddWithValue("@OrganizationId", organizationId.Value);
                    if (await updateCmd.ExecuteNonQueryAsync() == 0)
                        throw new InvalidOperationException("The recruiter's organization was not found.");
                }
                else
                {
                    const string insertQuery = @"
                        INSERT INTO Organization
                            (OrganizationName, Industry, Website, Logo, Address, CreatedAt, ApprovalStatus)
                        OUTPUT INSERTED.OrganizationId
                        VALUES
                            (@OrganizationName, @Industry, @Website, @Logo, @Address, GETDATE(), 'Draft')";
                    using var insertCmd = new SqlCommand(insertQuery, con, transaction);
                    AddOrganizationParameters(insertCmd, request);
                    organizationId = (int)(await insertCmd.ExecuteScalarAsync())!;

                    const string attachQuery = @"
                        UPDATE RecruiterProfile
                        SET OrganizationId = @OrganizationId, UpdatedAt = GETDATE()
                        WHERE RecruiterProfileId = @RecruiterProfileId";
                    using var attachCmd = new SqlCommand(attachQuery, con, transaction);
                    attachCmd.Parameters.AddWithValue("@OrganizationId", organizationId.Value);
                    attachCmd.Parameters.AddWithValue("@RecruiterProfileId", recruiterProfileId);
                    await attachCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return (await GetOrganizationAsync(userId))!;
        }

        public async Task<RecruiterOrganizationDto> SubmitOrganizationAsync(int userId)
        {
            const string query = @"
                UPDATE o
                SET ApprovalStatus = 'Pending', SubmittedAt = GETDATE(),
                    ReviewedAt = NULL, RejectionReason = NULL
                FROM Organization o
                JOIN RecruiterProfile rp ON rp.OrganizationId = o.OrganizationId
                WHERE rp.UserId = @UserId
                  AND o.ApprovalStatus IN ('Draft', 'Rejected')";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException(
                    "Only Draft or Rejected organizations can be submitted for approval.");

            return (await GetOrganizationAsync(userId))!;
        }

        public async Task<bool> IsOrganizationApprovedAsync(int userId)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM RecruiterProfile rp
                JOIN Organization o ON o.OrganizationId = rp.OrganizationId
                WHERE rp.UserId = @UserId AND o.ApprovalStatus = 'Approved'";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)(await cmd.ExecuteScalarAsync())! > 0;
        }

        public async Task<IEnumerable<RecruiterOrganizationDto>> GetPendingOrganizationsAsync()
        {
            const string query = @"
                SELECT OrganizationId, OrganizationName, Industry, Website, Logo, Address,
                       CreatedAt, ApprovalStatus, SubmittedAt, ReviewedAt, RejectionReason
                FROM Organization
                WHERE ApprovalStatus = 'Pending'
                ORDER BY SubmittedAt, OrganizationId";

            var organizations = new List<RecruiterOrganizationDto>();
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) organizations.Add(MapOrganization(reader));
            return organizations;
        }

        public async Task<RecruiterOrganizationDto?> ApproveOrganizationAsync(int organizationId)
        {
            const string query = @"
                UPDATE Organization
                SET ApprovalStatus = 'Approved', ReviewedAt = GETDATE(), RejectionReason = NULL
                WHERE OrganizationId = @OrganizationId AND ApprovalStatus = 'Pending'";

            if (!await UpdateApprovalAsync(query, organizationId)) return null;
            return await GetOrganizationByIdAsync(organizationId);
        }

        public async Task<RecruiterOrganizationDto?> RejectOrganizationAsync(
            int organizationId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A rejection reason is required.", nameof(reason));
            if (reason.Trim().Length > 1000)
                throw new ArgumentException("Rejection reason cannot exceed 1000 characters.", nameof(reason));

            const string query = @"
                UPDATE Organization
                SET ApprovalStatus = 'Rejected', ReviewedAt = GETDATE(), RejectionReason = @Reason
                WHERE OrganizationId = @OrganizationId AND ApprovalStatus = 'Pending'";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@OrganizationId", organizationId);
            cmd.Parameters.AddWithValue("@Reason", reason.Trim());
            if (await cmd.ExecuteNonQueryAsync() == 0) return null;
            return await GetOrganizationByIdAsync(organizationId);
        }

        private async Task<bool> UpdateApprovalAsync(string query, int organizationId)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@OrganizationId", organizationId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private async Task<RecruiterOrganizationDto?> GetOrganizationByIdAsync(int organizationId)
        {
            const string query = @"
                SELECT OrganizationId, OrganizationName, Industry, Website, Logo, Address,
                       CreatedAt, ApprovalStatus, SubmittedAt, ReviewedAt, RejectionReason
                FROM Organization
                WHERE OrganizationId = @OrganizationId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@OrganizationId", organizationId);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapOrganization(reader) : null;
        }

        private static void AddOrganizationParameters(
            SqlCommand cmd,
            UpsertRecruiterOrganizationRequest request)
        {
            cmd.Parameters.AddWithValue("@OrganizationName", request.OrganizationName.Trim());
            cmd.Parameters.AddWithValue("@Industry", (object?)request.Industry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Website", (object?)request.Website ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Logo", (object?)request.Logo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)request.Address ?? DBNull.Value);
        }

        private static RecruiterOrganizationDto MapOrganization(SqlDataReader reader)
        {
            return new RecruiterOrganizationDto
            {
                OrganizationId = (int)reader["OrganizationId"],
                OrganizationName = reader["OrganizationName"].ToString()!,
                Industry = reader["Industry"] == DBNull.Value ? null : reader["Industry"].ToString(),
                Website = reader["Website"] == DBNull.Value ? null : reader["Website"].ToString(),
                Logo = reader["Logo"] == DBNull.Value ? null : reader["Logo"].ToString(),
                Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"],
                ApprovalStatus = reader["ApprovalStatus"].ToString()!,
                SubmittedAt = reader["SubmittedAt"] == DBNull.Value ? null : (DateTime)reader["SubmittedAt"],
                ReviewedAt = reader["ReviewedAt"] == DBNull.Value ? null : (DateTime)reader["ReviewedAt"],
                RejectionReason = reader["RejectionReason"] == DBNull.Value
                    ? null
                    : reader["RejectionReason"].ToString()
            };
        }
    }
}
