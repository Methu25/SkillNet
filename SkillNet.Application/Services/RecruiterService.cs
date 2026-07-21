using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class RecruiterService : IRecruiterService
    {
        private const long MaximumLogoFileSize = 5 * 1024 * 1024;
        private static readonly IReadOnlyDictionary<string, string> AllowedLogoContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png",
                ["image/webp"] = ".webp"
            };

        private readonly string _connectionString;
        private readonly IProfileImageStorageService _imageStorageService;

        public RecruiterService(
            IConfiguration configuration,
            IProfileImageStorageService imageStorageService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _imageStorageService = imageStorageService;
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
                    RecruiterProfileId = (int)reader["RecruiterProfileId"],
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
                       o.Logo, o.Address, o.CreatedAt,
                       o.Description, o.CompanySize, o.FoundedYear, o.ContactEmail,
                       o.ContactPhone, o.LinkedInUrl, o.City, o.Country
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

            NormalizeOrganizationRequest(request);

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();

            try
            {
                int recruiterProfileId;
                int? organizationId;
                const string profileQuery = @"
                    SELECT rp.RecruiterProfileId, rp.OrganizationId
                    FROM RecruiterProfile rp
                    WHERE rp.UserId = @UserId";

                using (var profileCmd = new SqlCommand(profileQuery, con, transaction))
                {
                    profileCmd.Parameters.AddWithValue("@UserId", userId);
                    using var reader = await profileCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        recruiterProfileId = (int)reader["RecruiterProfileId"];
                        organizationId = reader["OrganizationId"] == DBNull.Value
                            ? null
                            : (int)reader["OrganizationId"];
                    }
                    else
                    {
                        recruiterProfileId = 0;
                        organizationId = null;
                    }
                }

                if (recruiterProfileId == 0)
                {
                    const string createProfileQuery = @"
                        INSERT INTO RecruiterProfile (UserId, CreatedAt, UpdatedAt)
                        OUTPUT INSERTED.RecruiterProfileId
                        VALUES (@UserId, GETDATE(), GETDATE())";
                    using var createProfileCmd = new SqlCommand(
                        createProfileQuery, con, transaction);
                    createProfileCmd.Parameters.AddWithValue("@UserId", userId);
                    recruiterProfileId = (int)(await createProfileCmd.ExecuteScalarAsync())!;
                }

                if (organizationId.HasValue)
                {
                    const string updateQuery = @"
                        UPDATE Organization
                        SET OrganizationName = @OrganizationName, Industry = @Industry,
                            Website = @Website, Logo = @Logo, Address = @Address,
                            Description = @Description, CompanySize = @CompanySize,
                            FoundedYear = @FoundedYear, ContactEmail = @ContactEmail,
                            ContactPhone = @ContactPhone, LinkedInUrl = @LinkedInUrl,
                            City = @City, Country = @Country
                        WHERE OrganizationId = @OrganizationId";
                    using var updateCmd = new SqlCommand(updateQuery, con, transaction);
                    AddOrganizationParameters(updateCmd, request);
                    updateCmd.Parameters.AddWithValue("@OrganizationId", organizationId.Value);
                    if (await updateCmd.ExecuteNonQueryAsync() == 0)
                        throw new KeyNotFoundException(
                            "The recruiter's organization was not found.");
                }
                else
                {
                    const string insertQuery = @"
                        INSERT INTO Organization
                            (OrganizationName, Industry, Website, Logo, Address, CreatedAt,
                             Description, CompanySize, FoundedYear, ContactEmail,
                             ContactPhone, LinkedInUrl, City, Country)
                        OUTPUT INSERTED.OrganizationId
                        VALUES
                            (@OrganizationName, @Industry, @Website, @Logo, @Address, GETDATE(),
                             @Description, @CompanySize, @FoundedYear, @ContactEmail,
                             @ContactPhone, @LinkedInUrl, @City, @Country)";
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
                    if (await attachCmd.ExecuteNonQueryAsync() == 0)
                        throw new InvalidOperationException(
                            "The recruiter profile changed while the organization was being saved.");
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

        public async Task<RecruiterOrganizationDto> UploadOrganizationLogoAsync(
            int userId,
            Stream content,
            string fileName,
            string contentType,
            long fileSize)
        {
            var extension = ValidateOrganizationLogo(
                content, fileName, contentType, fileSize);
            var ownedOrganization = await GetOwnedOrganizationLogoAsync(userId);
            var newLogoUrl = await _imageStorageService.SaveAsync(content, extension);

            try
            {
                const string updateQuery = @"
                    UPDATE o
                    SET Logo = @Logo
                    FROM Organization o
                    JOIN RecruiterProfile rp ON rp.OrganizationId = o.OrganizationId
                    WHERE rp.UserId = @UserId
                      AND o.OrganizationId = @OrganizationId";

                using var con = new SqlConnection(_connectionString);
                await con.OpenAsync();
                using var cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.AddWithValue("@Logo", newLogoUrl);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrganizationId", ownedOrganization.OrganizationId);
                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new UnauthorizedAccessException(
                        "The organization is not owned by the authenticated recruiter.");
            }
            catch
            {
                await _imageStorageService.DeleteAsync(newLogoUrl);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(ownedOrganization.Logo))
                await _imageStorageService.DeleteAsync(ownedOrganization.Logo);

            return (await GetOrganizationAsync(userId))
                ?? throw new KeyNotFoundException("Recruiter organization not found.");
        }

        public async Task<RecruiterOrganizationDto> DeleteOrganizationLogoAsync(int userId)
        {
            var ownedOrganization = await GetOwnedOrganizationLogoAsync(userId);

            const string updateQuery = @"
                UPDATE o
                SET Logo = NULL
                FROM Organization o
                JOIN RecruiterProfile rp ON rp.OrganizationId = o.OrganizationId
                WHERE rp.UserId = @UserId
                  AND o.OrganizationId = @OrganizationId";

            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using var cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OrganizationId", ownedOrganization.OrganizationId);
                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new UnauthorizedAccessException(
                        "The organization is not owned by the authenticated recruiter.");
            }

            if (!string.IsNullOrWhiteSpace(ownedOrganization.Logo))
                await _imageStorageService.DeleteAsync(ownedOrganization.Logo);

            return (await GetOrganizationAsync(userId))
                ?? throw new KeyNotFoundException("Recruiter organization not found.");
        }

        private async Task<RecruiterOrganizationDto?> GetOrganizationByIdAsync(int organizationId)
        {
            const string query = @"
                SELECT OrganizationId, OrganizationName, Industry, Website, Logo, Address, CreatedAt,
                       Description, CompanySize, FoundedYear, ContactEmail, ContactPhone, LinkedInUrl, City, Country
                FROM Organization
                WHERE OrganizationId = @OrganizationId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@OrganizationId", organizationId);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapOrganization(reader) : null;
        }

        private async Task<(int OrganizationId, string? Logo)> GetOwnedOrganizationLogoAsync(
            int userId)
        {
            const string query = @"
                SELECT o.OrganizationId, o.Logo
                FROM RecruiterProfile rp
                JOIN Organization o ON o.OrganizationId = rp.OrganizationId
                WHERE rp.UserId = @UserId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new KeyNotFoundException("Recruiter organization not found.");

            return (
                (int)reader["OrganizationId"],
                reader["Logo"] == DBNull.Value ? null : reader["Logo"].ToString());
        }

        private static string ValidateOrganizationLogo(
            Stream content,
            string fileName,
            string contentType,
            long fileSize)
        {
            if (content == null || content == Stream.Null || !content.CanRead || fileSize <= 0)
                throw new ArgumentException("A non-empty organization logo is required.");

            if (fileSize > MaximumLogoFileSize)
                throw new ArgumentOutOfRangeException(
                    nameof(fileSize),
                    $"Organization logo size cannot exceed {MaximumLogoFileSize} bytes.");

            if (!AllowedLogoContentTypes.TryGetValue(contentType, out var extension))
                throw new InvalidDataException(
                    "Only JPEG, PNG, and WEBP organization logos are supported.");

            var suppliedExtension = Path.GetExtension(fileName);
            var extensionMatches = extension == ".jpg"
                ? suppliedExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                  suppliedExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                : suppliedExtension.Equals(extension, StringComparison.OrdinalIgnoreCase);

            if (!extensionMatches || !HasValidImageSignature(content, extension))
                throw new InvalidDataException(
                    "The organization logo type or content is invalid.");

            return extension;
        }

        private static bool HasValidImageSignature(Stream content, string extension)
        {
            var originalPosition = content.CanSeek ? content.Position : 0;
            Span<byte> header = stackalloc byte[12];
            var bytesRead = content.Read(header);
            if (content.CanSeek) content.Position = originalPosition;

            return extension switch
            {
                ".jpg" => bytesRead >= 3 &&
                    header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 && header[..8].SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".webp" => bytesRead >= 12 &&
                    header[..4].SequenceEqual("RIFF"u8) &&
                    header[8..12].SequenceEqual("WEBP"u8),
                _ => false
            };
        }

        private static void AddOrganizationParameters(
            SqlCommand cmd,
            UpsertRecruiterOrganizationRequest request)
        {
            cmd.Parameters.Add("@OrganizationName", System.Data.SqlDbType.NVarChar, 200)
                .Value = request.OrganizationName;
            cmd.Parameters.Add("@Industry", System.Data.SqlDbType.NVarChar, 100)
                .Value = (object?)request.Industry ?? DBNull.Value;
            cmd.Parameters.Add("@Website", System.Data.SqlDbType.NVarChar, 255)
                .Value = (object?)request.Website ?? DBNull.Value;
            cmd.Parameters.Add("@Logo", System.Data.SqlDbType.NVarChar, 255)
                .Value = (object?)request.Logo ?? DBNull.Value;
            cmd.Parameters.Add("@Address", System.Data.SqlDbType.NVarChar, 500)
                .Value = (object?)request.Address ?? DBNull.Value;
            cmd.Parameters.Add("@Description", System.Data.SqlDbType.NVarChar, -1)
                .Value = (object?)request.Description ?? DBNull.Value;
            cmd.Parameters.Add("@CompanySize", System.Data.SqlDbType.NVarChar, 50)
                .Value = (object?)request.CompanySize ?? DBNull.Value;
            cmd.Parameters.Add("@FoundedYear", System.Data.SqlDbType.Int)
                .Value = (object?)request.FoundedYear ?? DBNull.Value;
            cmd.Parameters.Add("@ContactEmail", System.Data.SqlDbType.NVarChar, 254)
                .Value = (object?)request.ContactEmail ?? DBNull.Value;
            cmd.Parameters.Add("@ContactPhone", System.Data.SqlDbType.NVarChar, 30)
                .Value = (object?)request.ContactPhone ?? DBNull.Value;
            cmd.Parameters.Add("@LinkedInUrl", System.Data.SqlDbType.NVarChar, 255)
                .Value = (object?)request.LinkedInUrl ?? DBNull.Value;
            cmd.Parameters.Add("@City", System.Data.SqlDbType.NVarChar, 100)
                .Value = (object?)request.City ?? DBNull.Value;
            cmd.Parameters.Add("@Country", System.Data.SqlDbType.NVarChar, 100)
                .Value = (object?)request.Country ?? DBNull.Value;
        }

        private static void NormalizeOrganizationRequest(
            UpsertRecruiterOrganizationRequest request)
        {
            request.OrganizationName = request.OrganizationName.Trim();
            request.Industry = NormalizeOptional(request.Industry);
            request.Website = NormalizeOptional(request.Website);
            request.Logo = NormalizeOptional(request.Logo);
            request.Address = NormalizeOptional(request.Address);
            request.Description = NormalizeOptional(request.Description);
            request.CompanySize = NormalizeOptional(request.CompanySize);
            request.ContactEmail = NormalizeOptional(request.ContactEmail);
            request.ContactPhone = NormalizeOptional(request.ContactPhone);
            request.LinkedInUrl = NormalizeOptional(request.LinkedInUrl);
            request.City = NormalizeOptional(request.City);
            request.Country = NormalizeOptional(request.Country);
        }

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                CompanySize = reader["CompanySize"] == DBNull.Value ? null : reader["CompanySize"].ToString(),
                FoundedYear = reader["FoundedYear"] == DBNull.Value ? null : (int)reader["FoundedYear"],
                ContactEmail = reader["ContactEmail"] == DBNull.Value ? null : reader["ContactEmail"].ToString(),
                ContactPhone = reader["ContactPhone"] == DBNull.Value ? null : reader["ContactPhone"].ToString(),
                LinkedInUrl = reader["LinkedInUrl"] == DBNull.Value ? null : reader["LinkedInUrl"].ToString(),
                City = reader["City"] == DBNull.Value ? null : reader["City"].ToString(),
                Country = reader["Country"] == DBNull.Value ? null : reader["Country"].ToString()
            };
        }
    }
}
