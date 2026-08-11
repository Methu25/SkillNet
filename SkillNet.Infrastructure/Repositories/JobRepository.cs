using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly string _connectionString;

        public JobRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> InsertJobAsync(JobPost job)
        {
            const string query = @"
                INSERT INTO JobPost (RecruiterId, OrganizationId, CategoryId, Title, Description,
                    EmploymentType, WorkMode, Location, SalaryMin, SalaryMax, ExperienceLevel,
                    Status, ApplicationDeadline, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.JobId
                VALUES (@RecruiterId, @OrganizationId, @CategoryId, @Title, @Description,
                    @EmploymentType, @WorkMode, @Location, @SalaryMin, @SalaryMax, @ExperienceLevel,
                    @Status, @ApplicationDeadline, @CreatedAt, @UpdatedAt)";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RecruiterId", job.RecruiterId);
            cmd.Parameters.AddWithValue("@OrganizationId", job.OrganizationId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", job.CategoryId);
            cmd.Parameters.AddWithValue("@Title", job.Title);
            cmd.Parameters.AddWithValue("@Description", job.Description);
            cmd.Parameters.AddWithValue("@EmploymentType", job.EmploymentType);
            cmd.Parameters.AddWithValue("@WorkMode", job.WorkMode);
            cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryMin", job.SalaryMin ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryMax", job.SalaryMax ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceLevel", job.ExperienceLevel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", job.Status);
            cmd.Parameters.AddWithValue("@ApplicationDeadline", job.ApplicationDeadline ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", job.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", job.UpdatedAt);

            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<JobPost?> GetJobByIdAsync(int jobId)
        {
            const string query = "SELECT * FROM JobPost WHERE JobId = @JobId";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapJobPost(reader);
            return null;
        }

        public async Task<IEnumerable<JobPost>> GetJobsByRecruiterAsync(int recruiterProfileId)
        {
            const string query = "SELECT * FROM JobPost WHERE RecruiterId = @RecruiterId ORDER BY CreatedAt DESC";
            var jobs = new List<JobPost>();
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RecruiterId", recruiterProfileId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) jobs.Add(MapJobPost(reader));
            return jobs;
        }

        public async Task<IEnumerable<JobPost>> SearchJobsAsync(JobSearchRequest request)
        {
            var conditions = new List<string> { "Status = 'Published'" };
            var jobs = new List<JobPost>();

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var query = "SELECT * FROM JobPost WHERE ";

            if (!string.IsNullOrEmpty(request.Keyword))
                conditions.Add("(Title LIKE @Keyword OR Description LIKE @Keyword)");
            if (request.CategoryId.HasValue)
                conditions.Add("CategoryId = @CategoryId");
            if (!string.IsNullOrEmpty(request.WorkMode))
                conditions.Add("WorkMode = @WorkMode");
            if (!string.IsNullOrEmpty(request.Location))
                conditions.Add("Location LIKE @Location");
            if (request.SalaryMin.HasValue)
                conditions.Add("(SalaryMax IS NULL OR SalaryMax >= @SalaryMin)");
            if (request.SalaryMax.HasValue)
                conditions.Add("(SalaryMin IS NULL OR SalaryMin <= @SalaryMax)");
            if (!string.IsNullOrEmpty(request.ExperienceLevel))
                conditions.Add("ExperienceLevel = @ExperienceLevel");
            if (!string.IsNullOrEmpty(request.EmploymentType))
                conditions.Add("EmploymentType = @EmploymentType");

            query += string.Join(" AND ", conditions);

            query += request.SortBy switch
            {
                "salary-asc" => " ORDER BY SalaryMin ASC",
                "salary-desc" => " ORDER BY SalaryMax DESC",
                _ => " ORDER BY CreatedAt DESC"
            };

            query += $" OFFSET {(request.Page - 1) * request.PageSize} ROWS FETCH NEXT {request.PageSize} ROWS ONLY";

            using var cmd = new SqlCommand(query, con);
            if (!string.IsNullOrEmpty(request.Keyword))
                cmd.Parameters.AddWithValue("@Keyword", $"%{request.Keyword}%");
            if (request.CategoryId.HasValue)
                cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId.Value);
            if (!string.IsNullOrEmpty(request.WorkMode))
                cmd.Parameters.AddWithValue("@WorkMode", request.WorkMode);
            if (!string.IsNullOrEmpty(request.Location))
                cmd.Parameters.AddWithValue("@Location", $"%{request.Location}%");
            if (request.SalaryMin.HasValue)
                cmd.Parameters.AddWithValue("@SalaryMin", request.SalaryMin.Value);
            if (request.SalaryMax.HasValue)
                cmd.Parameters.AddWithValue("@SalaryMax", request.SalaryMax.Value);
            if (!string.IsNullOrEmpty(request.ExperienceLevel))
                cmd.Parameters.AddWithValue("@ExperienceLevel", request.ExperienceLevel);
            if (!string.IsNullOrEmpty(request.EmploymentType))
                cmd.Parameters.AddWithValue("@EmploymentType", request.EmploymentType);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) jobs.Add(MapJobPost(reader));
            return jobs;
        }

        public async Task<bool> UpdateJobAsync(JobPost job)
        {
            const string query = @"
                UPDATE JobPost SET CategoryId=@CategoryId, Title=@Title, Description=@Description,
                    EmploymentType=@EmploymentType, WorkMode=@WorkMode, Location=@Location,
                    SalaryMin=@SalaryMin, SalaryMax=@SalaryMax, ExperienceLevel=@ExperienceLevel,
                    ApplicationDeadline=@ApplicationDeadline, UpdatedAt=@UpdatedAt
                WHERE JobId=@JobId AND RecruiterId=@RecruiterId";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", job.JobId);
            cmd.Parameters.AddWithValue("@RecruiterId", job.RecruiterId);
            cmd.Parameters.AddWithValue("@CategoryId", job.CategoryId);
            cmd.Parameters.AddWithValue("@Title", job.Title);
            cmd.Parameters.AddWithValue("@Description", job.Description);
            cmd.Parameters.AddWithValue("@EmploymentType", job.EmploymentType);
            cmd.Parameters.AddWithValue("@WorkMode", job.WorkMode);
            cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryMin", job.SalaryMin ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SalaryMax", job.SalaryMax ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ExperienceLevel", job.ExperienceLevel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ApplicationDeadline", job.ApplicationDeadline ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteJobAsync(int jobId, int recruiterProfileId)
        {
            const string query = "DELETE FROM JobPost WHERE JobId=@JobId AND RecruiterId=@RecruiterId";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            cmd.Parameters.AddWithValue("@RecruiterId", recruiterProfileId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateJobStatusAsync(int jobId, int recruiterProfileId, string status)
        {
            const string query = "UPDATE JobPost SET Status=@Status, UpdatedAt=@UpdatedAt WHERE JobId=@JobId AND RecruiterId=@RecruiterId";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            cmd.Parameters.AddWithValue("@RecruiterId", recruiterProfileId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task InsertJobSkillsAsync(int jobId, List<int> skillIds)
        {
            if (!skillIds.Any()) return;
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            foreach (var skillId in skillIds)
            {
                const string query = "INSERT INTO JobSkill (JobId, SkillId) VALUES (@JobId, @SkillId)";
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@SkillId", skillId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteJobSkillsAsync(int jobId)
        {
            const string query = "DELETE FROM JobSkill WHERE JobId=@JobId";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<int>> GetSkillIdsByJobIdAsync(int jobId)
        {
            const string query = "SELECT SkillId FROM JobSkill WHERE JobId=@JobId ORDER BY SkillId";
            var skillIds = new List<int>();
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) skillIds.Add((int)reader["SkillId"]);
            return skillIds;
        }

        public async Task<IEnumerable<string>> GetSkillsByJobIdAsync(int jobId)
        {
            const string query = "SELECT s.SkillName FROM JobSkill js JOIN Skills s ON js.SkillId = s.SkillId WHERE js.JobId=@JobId";
            var skills = new List<string>();
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@JobId", jobId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) skills.Add(reader["SkillName"].ToString()!);
            return skills;
        }

        public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
        {
            const string query = "SELECT SkillId, SkillName FROM Skills ORDER BY SkillName";
            var skills = new List<SkillDto>();
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                skills.Add(new SkillDto
                {
                    SkillId = (int)reader["SkillId"],
                    SkillName = reader["SkillName"].ToString()!
                });
            }
            return skills;
        }

        public async Task<int> GetRecruiterOrganizationIdAsync(int recruiterProfileId)
        {
            const string query = "SELECT OrganizationId FROM RecruiterProfile WHERE RecruiterProfileId=@RecruiterProfileId";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RecruiterProfileId", recruiterProfileId);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? (int)result : 0;
        }

        public async Task<int> InsertJobWithSkillsAsync(JobPost job, List<int> skillIds)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();
            try
            {
                // 1. Insert JobPost
                const string query = @"
                    INSERT INTO JobPost (RecruiterId, OrganizationId, CategoryId, Title, Description,
                        EmploymentType, WorkMode, Location, SalaryMin, SalaryMax, ExperienceLevel,
                        Status, ApplicationDeadline, CreatedAt, UpdatedAt)
                    OUTPUT INSERTED.JobId
                    VALUES (@RecruiterId, @OrganizationId, @CategoryId, @Title, @Description,
                        @EmploymentType, @WorkMode, @Location, @SalaryMin, @SalaryMax, @ExperienceLevel,
                        @Status, @ApplicationDeadline, @CreatedAt, @UpdatedAt)";

                int jobId;
                using (var cmd = new SqlCommand(query, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@RecruiterId", job.RecruiterId);
                    cmd.Parameters.AddWithValue("@OrganizationId", job.OrganizationId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", job.CategoryId);
                    cmd.Parameters.AddWithValue("@Title", job.Title);
                    cmd.Parameters.AddWithValue("@Description", job.Description);
                    cmd.Parameters.AddWithValue("@EmploymentType", job.EmploymentType);
                    cmd.Parameters.AddWithValue("@WorkMode", job.WorkMode);
                    cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalaryMin", job.SalaryMin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalaryMax", job.SalaryMax ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExperienceLevel", job.ExperienceLevel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", job.Status);
                    cmd.Parameters.AddWithValue("@ApplicationDeadline", job.ApplicationDeadline ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", job.CreatedAt);
                    cmd.Parameters.AddWithValue("@UpdatedAt", job.UpdatedAt);

                    jobId = (int)(await cmd.ExecuteScalarAsync())!;
                }

                // 2. Insert JobSkills
                if (skillIds != null && skillIds.Any())
                {
                    var distinctSkillIds = skillIds.Distinct().ToList();

                    // Verify all skillIds exist in database
                    var parameters = new List<SqlParameter>();
                    var paramNames = new List<string>();
                    for (int i = 0; i < distinctSkillIds.Count; i++)
                    {
                        var paramName = $"@SkillId{i}";
                        paramNames.Add(paramName);
                        parameters.Add(new SqlParameter(paramName, distinctSkillIds[i]));
                    }
                    var formattedCheckQuery = string.Format("SELECT COUNT(1) FROM Skills WHERE SkillId IN ({0})", string.Join(",", paramNames));

                    using (var checkCmd = new SqlCommand(formattedCheckQuery, con, transaction))
                    {
                        checkCmd.Parameters.AddRange(parameters.ToArray());
                        var existCount = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                        if (existCount != distinctSkillIds.Count)
                        {
                            throw new ArgumentException("One or more provided Skill IDs do not exist.");
                        }
                    }

                    foreach (var skillId in distinctSkillIds)
                    {
                        const string skillQuery = "INSERT INTO JobSkill (JobId, SkillId) VALUES (@JobId, @SkillId)";
                        using var skillCmd = new SqlCommand(skillQuery, con, transaction);
                        skillCmd.Parameters.AddWithValue("@JobId", jobId);
                        skillCmd.Parameters.AddWithValue("@SkillId", skillId);
                        await skillCmd.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
                return jobId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateJobWithSkillsAsync(JobPost job, List<int> skillIds)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var transaction = con.BeginTransaction();
            try
            {
                // 1. Update JobPost
                const string query = @"
                    UPDATE JobPost SET CategoryId=@CategoryId, Title=@Title, Description=@Description,
                        EmploymentType=@EmploymentType, WorkMode=@WorkMode, Location=@Location,
                        SalaryMin=@SalaryMin, SalaryMax=@SalaryMax, ExperienceLevel=@ExperienceLevel,
                        ApplicationDeadline=@ApplicationDeadline, UpdatedAt=@UpdatedAt
                    WHERE JobId=@JobId AND RecruiterId=@RecruiterId";

                using (var cmd = new SqlCommand(query, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@JobId", job.JobId);
                    cmd.Parameters.AddWithValue("@RecruiterId", job.RecruiterId);
                    cmd.Parameters.AddWithValue("@CategoryId", job.CategoryId);
                    cmd.Parameters.AddWithValue("@Title", job.Title);
                    cmd.Parameters.AddWithValue("@Description", job.Description);
                    cmd.Parameters.AddWithValue("@EmploymentType", job.EmploymentType);
                    cmd.Parameters.AddWithValue("@WorkMode", job.WorkMode);
                    cmd.Parameters.AddWithValue("@Location", job.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalaryMin", job.SalaryMin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SalaryMax", job.SalaryMax ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExperienceLevel", job.ExperienceLevel ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicationDeadline", job.ApplicationDeadline ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                // 2. Update JobSkills
                if (skillIds != null)
                {
                    const string deleteQuery = "DELETE FROM JobSkill WHERE JobId=@JobId";
                    using (var delCmd = new SqlCommand(deleteQuery, con, transaction))
                    {
                        delCmd.Parameters.AddWithValue("@JobId", job.JobId);
                        await delCmd.ExecuteNonQueryAsync();
                    }

                    if (skillIds.Any())
                    {
                        var distinctSkillIds = skillIds.Distinct().ToList();

                        var parameters = new List<SqlParameter>();
                        var paramNames = new List<string>();
                        for (int i = 0; i < distinctSkillIds.Count; i++)
                        {
                            var paramName = $"@SkillId{i}";
                            paramNames.Add(paramName);
                            parameters.Add(new SqlParameter(paramName, distinctSkillIds[i]));
                        }
                        var formattedCheckQuery = string.Format("SELECT COUNT(1) FROM Skills WHERE SkillId IN ({0})", string.Join(",", paramNames));

                        using (var checkCmd = new SqlCommand(formattedCheckQuery, con, transaction))
                        {
                            checkCmd.Parameters.AddRange(parameters.ToArray());
                            var existCount = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                            if (existCount != distinctSkillIds.Count)
                            {
                                throw new ArgumentException("One or more provided Skill IDs do not exist.");
                            }
                        }

                        foreach (var skillId in distinctSkillIds)
                        {
                            const string skillQuery = "INSERT INTO JobSkill (JobId, SkillId) VALUES (@JobId, @SkillId)";
                            using var skillCmd = new SqlCommand(skillQuery, con, transaction);
                            skillCmd.Parameters.AddWithValue("@JobId", job.JobId);
                            skillCmd.Parameters.AddWithValue("@SkillId", skillId);
                            await skillCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<JobPost>> GetActiveJobsAsync()
        {
            var jobs = new List<JobPost>();
            const string query = "SELECT * FROM JobPost WHERE Status = 'Published' ORDER BY CreatedAt DESC";
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                jobs.Add(MapJobPost(reader));
            }
            return jobs;
        }

        public async Task<System.Linq.ILookup<int, SkillDto>> GetActiveJobSkillsAsync()
        {
            var list = new List<KeyValuePair<int, SkillDto>>();
            const string query = @"
                SELECT js.JobId, s.SkillId, s.SkillName 
                FROM JobSkill js 
                JOIN Skills s ON js.SkillId = s.SkillId 
                WHERE js.JobId IN (SELECT JobId FROM JobPost WHERE Status = 'Published')";

            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var jobId = (int)reader["JobId"];
                var skill = new SkillDto
                {
                    SkillId = (int)reader["SkillId"],
                    SkillName = reader["SkillName"].ToString()!
                };
                list.Add(new KeyValuePair<int, SkillDto>(jobId, skill));
            }
            return list.ToLookup(x => x.Key, x => x.Value);
        }

        private static JobPost MapJobPost(SqlDataReader r) => new()
        {
            JobId = (int)r["JobId"],
            RecruiterId = (int)r["RecruiterId"],
            OrganizationId = r["OrganizationId"] == DBNull.Value ? null : (int)r["OrganizationId"],
            CategoryId = (int)r["CategoryId"],
            Title = r["Title"].ToString()!,
            Description = r["Description"].ToString()!,
            EmploymentType = r["EmploymentType"].ToString()!,
            WorkMode = r["WorkMode"].ToString()!,
            Location = r["Location"] == DBNull.Value ? string.Empty : r["Location"].ToString()!,
            SalaryMin = r["SalaryMin"] == DBNull.Value ? null : (decimal)r["SalaryMin"],
            SalaryMax = r["SalaryMax"] == DBNull.Value ? null : (decimal)r["SalaryMax"],
            ExperienceLevel = r["ExperienceLevel"] == DBNull.Value ? string.Empty : r["ExperienceLevel"].ToString()!,
            Status = r["Status"].ToString()!,
            ApplicationDeadline = r["ApplicationDeadline"] == DBNull.Value ? null : (DateTime)r["ApplicationDeadline"],
            CreatedAt = (DateTime)r["CreatedAt"],
            UpdatedAt = (DateTime)r["UpdatedAt"]
        };
    }
}
