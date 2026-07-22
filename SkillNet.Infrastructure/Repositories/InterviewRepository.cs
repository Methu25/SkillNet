using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly string _connectionString;

        public InterviewRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private Interview MapInterview(SqlDataReader reader)
        {
            return new Interview
            {
                InterviewId = reader.GetInt32(reader.GetOrdinal("InterviewId")),
                ApplicationId = reader.GetInt32(reader.GetOrdinal("ApplicationId")),
                InterviewType = reader.IsDBNull(reader.GetOrdinal("InterviewType")) ? null : reader.GetString(reader.GetOrdinal("InterviewType")),
                InterviewRound = reader.GetInt32(reader.GetOrdinal("InterviewRound")),
                ScheduledDate = reader.GetDateTime(reader.GetOrdinal("ScheduledDate")),
                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                MeetingLink = reader.IsDBNull(reader.GetOrdinal("MeetingLink")) ? null : reader.GetString(reader.GetOrdinal("MeetingLink")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public async Task<IEnumerable<Interview>> GetAllInterviewsAsync()
        {
            var interviews = new List<Interview>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM Interview", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        interviews.Add(MapInterview(reader));
                }
            }
            return interviews;
        }

        public async Task<Interview?> GetInterviewByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM Interview WHERE InterviewId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return MapInterview(reader);
                }
            }
            return null;
        }

        public async Task<Interview> CreateInterviewAsync(Interview interview)
        {
            interview.CreatedAt = DateTime.Now;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    INSERT INTO Interview 
                    (ApplicationId, InterviewType, InterviewRound, ScheduledDate, Duration, Location, MeetingLink, Status, CreatedAt) 
                    OUTPUT INSERTED.InterviewId 
                    VALUES 
                    (@ApplicationId, @InterviewType, @InterviewRound, @ScheduledDate, @Duration, @Location, @MeetingLink, @Status, @CreatedAt)";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                command.Parameters.AddWithValue("@InterviewType", (object?)interview.InterviewType ?? DBNull.Value);
                command.Parameters.AddWithValue("@InterviewRound", interview.InterviewRound);
                command.Parameters.AddWithValue("@ScheduledDate", interview.ScheduledDate);
                command.Parameters.AddWithValue("@Duration", interview.Duration);
                command.Parameters.AddWithValue("@Location", (object?)interview.Location ?? DBNull.Value);
                command.Parameters.AddWithValue("@MeetingLink", (object?)interview.MeetingLink ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)interview.Status ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", interview.CreatedAt);

                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Failed to create the interview.");

                interview.InterviewId = Convert.ToInt32(result);
            }
            return interview;
        }

        public async Task<InterviewSchedulingContext?> GetSchedulingContextAsync(int applicationId)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            const string query = @"
                SELECT ja.ApplicationId, ja.CurrentStatus, rp.UserId AS RecruiterUserId
                FROM JobApplications ja
                INNER JOIN JobPost jp ON jp.JobId = ja.JobId
                INNER JOIN RecruiterProfile rp ON rp.RecruiterProfileId = jp.RecruiterId
                WHERE ja.ApplicationId = @ApplicationId";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ApplicationId", applicationId);
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return new InterviewSchedulingContext
            {
                ApplicationId = reader.GetInt32(reader.GetOrdinal("ApplicationId")),
                RecruiterUserId = reader.GetInt32(reader.GetOrdinal("RecruiterUserId")),
                CurrentStatus = reader.GetString(reader.GetOrdinal("CurrentStatus"))
            };
        }

        public async Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync()
        {
            var interviewers = new List<EligibleInterviewerResponse>();
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            const string query = @"
                SELECT DISTINCT i.InterviewerId,
                       CONCAT(u.FirstName, ' ', u.LastName) AS Name,
                       i.Position
                FROM Interviewer i
                INNER JOIN Users u ON u.UserId = i.UserId
                INNER JOIN UserRole ur ON ur.UserId = u.UserId
                INNER JOIN Roles r ON r.RoleId = ur.RoleId
                WHERE r.RoleName = 'HiringManager' AND u.Status = 'Active'
                ORDER BY Name";
            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                interviewers.Add(new EligibleInterviewerResponse
                {
                    InterviewerId = reader.GetInt32(reader.GetOrdinal("InterviewerId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? null : reader.GetString(reader.GetOrdinal("Position"))
                });
            }
            return interviewers;
        }

        public async Task<Interview> CreateScheduledInterviewAsync(
            Interview interview,
            IReadOnlyCollection<int> interviewerIds,
            int changedByUserId,
            string? note)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                const string applicationQuery = @"
                    SELECT ja.CurrentStatus, rp.UserId AS RecruiterUserId
                    FROM JobApplications ja WITH (UPDLOCK, HOLDLOCK)
                    INNER JOIN JobPost jp ON jp.JobId = ja.JobId
                    INNER JOIN RecruiterProfile rp ON rp.RecruiterProfileId = jp.RecruiterId
                    WHERE ja.ApplicationId = @ApplicationId";
                await using (var applicationCommand = new SqlCommand(applicationQuery, connection, transaction))
                {
                    applicationCommand.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                    await using var reader = await applicationCommand.ExecuteReaderAsync();
                    if (!await reader.ReadAsync()) throw new KeyNotFoundException("Application not found.");
                    if (reader.GetInt32(reader.GetOrdinal("RecruiterUserId")) != changedByUserId)
                        throw new UnauthorizedAccessException("This application is not owned by the authenticated recruiter.");
                    if (!string.Equals(reader.GetString(reader.GetOrdinal("CurrentStatus")), "Shortlisted", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Only Shortlisted applications can be scheduled for an interview.");
                }

                const string duplicateQuery = @"
                    SELECT COUNT(*) FROM Interview WITH (UPDLOCK, HOLDLOCK)
                    WHERE ApplicationId = @ApplicationId AND Status IN ('Scheduled', 'Rescheduled')";
                await using (var duplicateCommand = new SqlCommand(duplicateQuery, connection, transaction))
                {
                    duplicateCommand.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                    if (Convert.ToInt32(await duplicateCommand.ExecuteScalarAsync()) > 0)
                        throw new InvalidOperationException("An active interview already exists for this application.");
                }

                var distinctIds = interviewerIds.Distinct().ToArray();
                var parameterNames = distinctIds.Select((_, index) => $"@InterviewerId{index}").ToArray();
                var interviewerQuery = $@"
                    SELECT COUNT(DISTINCT i.InterviewerId)
                    FROM Interviewer i
                    INNER JOIN Users u ON u.UserId = i.UserId
                    INNER JOIN UserRole ur ON ur.UserId = u.UserId
                    INNER JOIN Roles r ON r.RoleId = ur.RoleId
                    WHERE i.InterviewerId IN ({string.Join(",", parameterNames)})
                      AND r.RoleName = 'HiringManager' AND u.Status = 'Active'";
                await using (var interviewerCommand = new SqlCommand(interviewerQuery, connection, transaction))
                {
                    for (var index = 0; index < distinctIds.Length; index++)
                        interviewerCommand.Parameters.AddWithValue(parameterNames[index], distinctIds[index]);
                    if (Convert.ToInt32(await interviewerCommand.ExecuteScalarAsync()) != distinctIds.Length)
                        throw new KeyNotFoundException("One or more interviewers are missing or ineligible.");
                }

                const string insertInterview = @"
                    INSERT INTO Interview
                        (ApplicationId, InterviewType, InterviewRound, ScheduledDate, Duration, Location, MeetingLink, Status, CreatedAt)
                    OUTPUT INSERTED.InterviewId
                    VALUES
                        (@ApplicationId, @InterviewType, @InterviewRound, @ScheduledDate, @Duration, @Location, @MeetingLink, @Status, @CreatedAt)";
                await using (var interviewCommand = new SqlCommand(insertInterview, connection, transaction))
                {
                    interviewCommand.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                    interviewCommand.Parameters.AddWithValue("@InterviewType", (object?)interview.InterviewType ?? DBNull.Value);
                    interviewCommand.Parameters.AddWithValue("@InterviewRound", interview.InterviewRound);
                    interviewCommand.Parameters.AddWithValue("@ScheduledDate", interview.ScheduledDate);
                    interviewCommand.Parameters.AddWithValue("@Duration", interview.Duration);
                    interviewCommand.Parameters.AddWithValue("@Location", (object?)interview.Location ?? DBNull.Value);
                    interviewCommand.Parameters.AddWithValue("@MeetingLink", (object?)interview.MeetingLink ?? DBNull.Value);
                    interviewCommand.Parameters.AddWithValue("@Status", interview.Status ?? "Scheduled");
                    interviewCommand.Parameters.AddWithValue("@CreatedAt", interview.CreatedAt);
                    interview.InterviewId = Convert.ToInt32(await interviewCommand.ExecuteScalarAsync());
                }

                foreach (var interviewerId in distinctIds)
                {
                    await using var assignmentCommand = new SqlCommand(
                        "INSERT INTO InterviewAssignment (InterviewId, InterviewerId, Role) VALUES (@InterviewId, @InterviewerId, @Role)",
                        connection, transaction);
                    assignmentCommand.Parameters.AddWithValue("@InterviewId", interview.InterviewId);
                    assignmentCommand.Parameters.AddWithValue("@InterviewerId", interviewerId);
                    assignmentCommand.Parameters.AddWithValue("@Role", "Interviewer");
                    await assignmentCommand.ExecuteNonQueryAsync();
                }

                await using (var statusCommand = new SqlCommand(@"
                    UPDATE JobApplications
                    SET CurrentStatus = 'Interviewing', LastUpdated = @ChangedAt
                    WHERE ApplicationId = @ApplicationId AND CurrentStatus = 'Shortlisted'", connection, transaction))
                {
                    statusCommand.Parameters.AddWithValue("@ChangedAt", interview.CreatedAt);
                    statusCommand.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                    if (await statusCommand.ExecuteNonQueryAsync() != 1)
                        throw new InvalidOperationException("The application status changed before the interview could be scheduled.");
                }

                await using (var historyCommand = new SqlCommand(@"
                    INSERT INTO ApplicationStatusHistories
                        (ApplicationId, OldStatus, NewStatus, ChangedBy, ChangedAt, Comment)
                    VALUES
                        (@ApplicationId, 'Shortlisted', 'Interviewing', @ChangedBy, @ChangedAt, @Comment)", connection, transaction))
                {
                    historyCommand.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                    historyCommand.Parameters.AddWithValue("@ChangedBy", changedByUserId);
                    historyCommand.Parameters.AddWithValue("@ChangedAt", interview.CreatedAt);
                    historyCommand.Parameters.AddWithValue("@Comment", (object?)note ?? "Interview scheduled.");
                    await historyCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return interview;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Interview?> UpdateInterviewAsync(int id, Interview interview)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    UPDATE Interview SET 
                    ApplicationId = @ApplicationId, 
                    InterviewType = @InterviewType, 
                    InterviewRound = @InterviewRound, 
                    ScheduledDate = @ScheduledDate, 
                    Duration = @Duration, 
                    Location = @Location, 
                    MeetingLink = @MeetingLink, 
                    Status = @Status 
                    WHERE InterviewId = @Id";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                command.Parameters.AddWithValue("@InterviewType", (object?)interview.InterviewType ?? DBNull.Value);
                command.Parameters.AddWithValue("@InterviewRound", interview.InterviewRound);
                command.Parameters.AddWithValue("@ScheduledDate", interview.ScheduledDate);
                command.Parameters.AddWithValue("@Duration", interview.Duration);
                command.Parameters.AddWithValue("@Location", (object?)interview.Location ?? DBNull.Value);
                command.Parameters.AddWithValue("@MeetingLink", (object?)interview.MeetingLink ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)interview.Status ?? DBNull.Value);
                command.Parameters.AddWithValue("@Id", id);

                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0) return null;
            }
            interview.InterviewId = id;
            return interview;
        }

        public async Task<bool> DeleteInterviewAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("DELETE FROM Interview WHERE InterviewId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                var rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<Interview?> UpdateInterviewStatusAsync(int id, string status)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("UPDATE Interview SET Status = @Status WHERE InterviewId = @Id", connection);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@Id", id);
                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0) return null;
            }
            return await GetInterviewByIdAsync(id);
        }

        private InterviewEvaluation MapEvaluation(SqlDataReader reader)
        {
            return new InterviewEvaluation
            {
                EvaluationId = reader.GetInt32(reader.GetOrdinal("EvaluationId")),
                InterviewId = reader.GetInt32(reader.GetOrdinal("InterviewId")),
                InterviewerId = reader.GetInt32(reader.GetOrdinal("InterviewerId")),
                TechnicalScore = reader.GetInt32(reader.GetOrdinal("TechnicalScore")),
                CommunicationScore = reader.GetInt32(reader.GetOrdinal("CommunicationScore")),
                ProblemSolvingScore = reader.GetInt32(reader.GetOrdinal("ProblemSolvingScore")),
                CultureFitScore = reader.GetInt32(reader.GetOrdinal("CultureFitScore")),
                OverallScore = Convert.ToDecimal(reader["OverallScore"]),
                Recommendation = reader.IsDBNull(reader.GetOrdinal("Recommendation")) ? null : reader.GetString(reader.GetOrdinal("Recommendation")),
                Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt"))
            };
        }

        public async Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation)
        {
            evaluation.SubmittedAt = DateTime.Now;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    INSERT INTO InterviewEvaluation 
                    (InterviewId, InterviewerId, TechnicalScore, CommunicationScore, ProblemSolvingScore, CultureFitScore, OverallScore, Recommendation, Comments, SubmittedAt) 
                    OUTPUT INSERTED.EvaluationId 
                    VALUES 
                    (@InterviewId, @InterviewerId, @TechnicalScore, @CommunicationScore, @ProblemSolvingScore, @CultureFitScore, @OverallScore, @Recommendation, @Comments, @SubmittedAt)";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InterviewId", evaluation.InterviewId);
                command.Parameters.AddWithValue("@InterviewerId", evaluation.InterviewerId);
                command.Parameters.AddWithValue("@TechnicalScore", evaluation.TechnicalScore);
                command.Parameters.AddWithValue("@CommunicationScore", evaluation.CommunicationScore);
                command.Parameters.AddWithValue("@ProblemSolvingScore", evaluation.ProblemSolvingScore);
                command.Parameters.AddWithValue("@CultureFitScore", evaluation.CultureFitScore);
                command.Parameters.AddWithValue("@OverallScore", evaluation.OverallScore);
                command.Parameters.AddWithValue("@Recommendation", (object?)evaluation.Recommendation ?? DBNull.Value);
                command.Parameters.AddWithValue("@Comments", (object?)evaluation.Comments ?? DBNull.Value);
                command.Parameters.AddWithValue("@SubmittedAt", evaluation.SubmittedAt);

                evaluation.EvaluationId = (int)(await command.ExecuteScalarAsync() ?? 0);
            }
            return evaluation;
        }

        public async Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM InterviewEvaluation WHERE InterviewId = @InterviewId", connection);
                command.Parameters.AddWithValue("@InterviewId", interviewId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return MapEvaluation(reader);
                }
            }
            return null;
        }

        public async Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation)
        {
            evaluation.SubmittedAt = DateTime.Now;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get old value for audit trail
                var oldEvaluation = await GetEvaluationByInterviewIdAsync(interviewId);
                if (oldEvaluation != null)
                {
                    var historyQuery = @"
                        INSERT INTO InterviewFeedbackHistory (EvaluationId, UpdatedBy, OldValue, NewValue, UpdatedAt)
                        VALUES (@EvaluationId, @UpdatedBy, @OldValue, @NewValue, @UpdatedAt)";
                    var historyCommand = new SqlCommand(historyQuery, connection);
                    historyCommand.Parameters.AddWithValue("@EvaluationId", oldEvaluation.EvaluationId);
                    historyCommand.Parameters.AddWithValue("@UpdatedBy", evaluation.InterviewerId);
                    historyCommand.Parameters.AddWithValue("@OldValue", $"Technical: {oldEvaluation.TechnicalScore}, Comm: {oldEvaluation.CommunicationScore}, Problem: {oldEvaluation.ProblemSolvingScore}, Fit: {oldEvaluation.CultureFitScore}, Rec: {oldEvaluation.Recommendation}");
                    historyCommand.Parameters.AddWithValue("@NewValue", $"Technical: {evaluation.TechnicalScore}, Comm: {evaluation.CommunicationScore}, Problem: {evaluation.ProblemSolvingScore}, Fit: {evaluation.CultureFitScore}, Rec: {evaluation.Recommendation}");
                    historyCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                    await historyCommand.ExecuteNonQueryAsync();
                }

                var query = @"
                    UPDATE InterviewEvaluation SET 
                    InterviewerId = @InterviewerId, 
                    TechnicalScore = @TechnicalScore, 
                    CommunicationScore = @CommunicationScore, 
                    ProblemSolvingScore = @ProblemSolvingScore, 
                    CultureFitScore = @CultureFitScore, 
                    OverallScore = @OverallScore, 
                    Recommendation = @Recommendation, 
                    Comments = @Comments, 
                    SubmittedAt = @SubmittedAt 
                    WHERE InterviewId = @InterviewId";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InterviewerId", evaluation.InterviewerId);
                command.Parameters.AddWithValue("@TechnicalScore", evaluation.TechnicalScore);
                command.Parameters.AddWithValue("@CommunicationScore", evaluation.CommunicationScore);
                command.Parameters.AddWithValue("@ProblemSolvingScore", evaluation.ProblemSolvingScore);
                command.Parameters.AddWithValue("@CultureFitScore", evaluation.CultureFitScore);
                command.Parameters.AddWithValue("@OverallScore", evaluation.OverallScore);
                command.Parameters.AddWithValue("@Recommendation", (object?)evaluation.Recommendation ?? DBNull.Value);
                command.Parameters.AddWithValue("@Comments", (object?)evaluation.Comments ?? DBNull.Value);
                command.Parameters.AddWithValue("@SubmittedAt", evaluation.SubmittedAt);
                command.Parameters.AddWithValue("@InterviewId", interviewId);

                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0) return null;
            }
            evaluation.InterviewId = interviewId;
            return evaluation;
        }

        public async Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync()
        {
            var interviews = new List<Interview>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM Interview WHERE ScheduledDate >= @Now", connection);
                command.Parameters.AddWithValue("@Now", DateTime.Now);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        interviews.Add(MapInterview(reader));
                }
            }
            return interviews;
        }

        public async Task<IEnumerable<Interview>> GetAssignedInterviewsAsync(int hiringManagerUserId)
        {
            var interviews = new List<Interview>();
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            const string query = @"
                SELECT i.*, ja.CurrentStatus AS ApplicationStatus,
                       CONCAT(c.FirstName, ' ', c.LastName) AS CandidateName,
                       u.Email AS CandidateEmail, jp.Title AS JobTitle,
                       c.ProfessionalSummary AS CandidateSummary, c.ExperienceYears,
                       CAST(CASE WHEN EXISTS (
                           SELECT 1 FROM InterviewEvaluation ie WHERE ie.InterviewId = i.InterviewId
                       ) THEN 1 ELSE 0 END AS bit) AS HasEvaluation
                FROM Interview i
                INNER JOIN InterviewAssignment ia ON ia.InterviewId = i.InterviewId
                INNER JOIN Interviewer interviewer ON interviewer.InterviewerId = ia.InterviewerId
                INNER JOIN JobApplications ja ON ja.ApplicationId = i.ApplicationId
                INNER JOIN Candidates c ON c.UserId = ja.CandidateId
                INNER JOIN Users u ON u.UserId = c.UserId
                INNER JOIN JobPost jp ON jp.JobId = ja.JobId
                WHERE interviewer.UserId = @UserId
                ORDER BY i.ScheduledDate DESC";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", hiringManagerUserId);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) interviews.Add(MapAssignedInterview(reader));
            return interviews;
        }

        public async Task<Interview?> GetAssignedInterviewAsync(int interviewId, int hiringManagerUserId)
        {
            var interviews = await GetAssignedInterviewsAsync(hiringManagerUserId);
            return interviews.SingleOrDefault(interview => interview.InterviewId == interviewId);
        }

        private Interview MapAssignedInterview(SqlDataReader reader)
        {
            var interview = MapInterview(reader);
            interview.ApplicationStatus = reader.GetString(reader.GetOrdinal("ApplicationStatus"));
            interview.CandidateName = reader.GetString(reader.GetOrdinal("CandidateName"));
            interview.CandidateEmail = reader.GetString(reader.GetOrdinal("CandidateEmail"));
            interview.JobTitle = reader.GetString(reader.GetOrdinal("JobTitle"));
            interview.CandidateSummary = reader.IsDBNull(reader.GetOrdinal("CandidateSummary")) ? null : reader.GetString(reader.GetOrdinal("CandidateSummary"));
            interview.ExperienceYears = reader.IsDBNull(reader.GetOrdinal("ExperienceYears")) ? null : reader.GetInt32(reader.GetOrdinal("ExperienceYears"));
            interview.HasEvaluation = reader.GetBoolean(reader.GetOrdinal("HasEvaluation"));
            return interview;
        }

        public async Task<InterviewEvaluation> CreateEvaluationAndTransitionAsync(InterviewEvaluation evaluation, int hiringManagerUserId)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var context = await GetEvaluationContextAsync(connection, transaction, evaluation.InterviewId, hiringManagerUserId);
                if (!context.Exists) throw new KeyNotFoundException("Interview not found.");
                if (context.InterviewerId == null) throw new UnauthorizedAccessException("The Hiring Manager is not assigned to this interview.");
                if (!string.Equals(context.ApplicationStatus, "Interviewing", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The application must be Interviewing before evaluation.");

                await using (var duplicate = new SqlCommand("SELECT COUNT(*) FROM InterviewEvaluation WITH (UPDLOCK, HOLDLOCK) WHERE InterviewId = @InterviewId", connection, transaction))
                {
                    duplicate.Parameters.AddWithValue("@InterviewId", evaluation.InterviewId);
                    if (Convert.ToInt32(await duplicate.ExecuteScalarAsync()) > 0)
                        throw new InvalidOperationException("An evaluation has already been submitted for this interview.");
                }

                evaluation.InterviewerId = context.InterviewerId.Value;
                await using (var insert = new SqlCommand(@"
                    INSERT INTO InterviewEvaluation
                        (InterviewId, InterviewerId, TechnicalScore, CommunicationScore, ProblemSolvingScore, CultureFitScore, OverallScore, Recommendation, Comments, SubmittedAt)
                    OUTPUT INSERTED.EvaluationId
                    VALUES
                        (@InterviewId, @InterviewerId, @TechnicalScore, @CommunicationScore, @ProblemSolvingScore, @CultureFitScore, @OverallScore, @Recommendation, @Comments, @SubmittedAt)", connection, transaction))
                {
                    insert.Parameters.AddWithValue("@InterviewId", evaluation.InterviewId);
                    insert.Parameters.AddWithValue("@InterviewerId", evaluation.InterviewerId);
                    insert.Parameters.AddWithValue("@TechnicalScore", evaluation.TechnicalScore);
                    insert.Parameters.AddWithValue("@CommunicationScore", evaluation.CommunicationScore);
                    insert.Parameters.AddWithValue("@ProblemSolvingScore", evaluation.ProblemSolvingScore);
                    insert.Parameters.AddWithValue("@CultureFitScore", evaluation.CultureFitScore);
                    insert.Parameters.AddWithValue("@OverallScore", evaluation.OverallScore);
                    insert.Parameters.AddWithValue("@Recommendation", evaluation.Recommendation!);
                    insert.Parameters.AddWithValue("@Comments", evaluation.Comments!);
                    insert.Parameters.AddWithValue("@SubmittedAt", evaluation.SubmittedAt);
                    evaluation.EvaluationId = Convert.ToInt32(await insert.ExecuteScalarAsync());
                }

                await using (var update = new SqlCommand(@"
                    UPDATE JobApplications SET CurrentStatus = 'EvaluationSubmitted', LastUpdated = @ChangedAt
                    WHERE ApplicationId = @ApplicationId AND CurrentStatus = 'Interviewing';
                    UPDATE Interview SET Status = 'EvaluationSubmitted' WHERE InterviewId = @InterviewId;", connection, transaction))
                {
                    update.Parameters.AddWithValue("@ChangedAt", evaluation.SubmittedAt);
                    update.Parameters.AddWithValue("@ApplicationId", context.ApplicationId);
                    update.Parameters.AddWithValue("@InterviewId", evaluation.InterviewId);
                    await update.ExecuteNonQueryAsync();
                }

                await InsertStatusHistoryAsync(connection, transaction, context.ApplicationId, "Interviewing", "EvaluationSubmitted", hiringManagerUserId, evaluation.SubmittedAt, "Interview evaluation submitted.");
                transaction.Commit();
                return evaluation;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<string> RecordDecisionAsync(int interviewId, int hiringManagerUserId, string decision)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var context = await GetEvaluationContextAsync(connection, transaction, interviewId, hiringManagerUserId);
                if (!context.Exists) throw new KeyNotFoundException("Interview not found.");
                if (context.InterviewerId == null) throw new UnauthorizedAccessException("The Hiring Manager is not assigned to this interview.");
                if (string.Equals(context.ApplicationStatus, decision, StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Commit();
                    return decision;
                }
                if (context.ApplicationStatus is "Hired" or "Rejected")
                    throw new InvalidOperationException("A terminal hiring decision cannot be reversed.");
                if (!string.Equals(context.ApplicationStatus, "EvaluationSubmitted", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("An evaluation must be submitted before recording a hiring decision.");

                await using (var evaluationCommand = new SqlCommand("SELECT COUNT(*) FROM InterviewEvaluation WHERE InterviewId = @InterviewId", connection, transaction))
                {
                    evaluationCommand.Parameters.AddWithValue("@InterviewId", interviewId);
                    if (Convert.ToInt32(await evaluationCommand.ExecuteScalarAsync()) == 0)
                        throw new InvalidOperationException("An evaluation must be submitted before recording a hiring decision.");
                }

                var changedAt = DateTime.UtcNow;
                await using (var update = new SqlCommand(@"
                    UPDATE JobApplications SET CurrentStatus = @Decision, LastUpdated = @ChangedAt
                    WHERE ApplicationId = @ApplicationId AND CurrentStatus = 'EvaluationSubmitted';
                    UPDATE Interview SET Status = @Decision WHERE InterviewId = @InterviewId;", connection, transaction))
                {
                    update.Parameters.AddWithValue("@Decision", decision);
                    update.Parameters.AddWithValue("@ChangedAt", changedAt);
                    update.Parameters.AddWithValue("@ApplicationId", context.ApplicationId);
                    update.Parameters.AddWithValue("@InterviewId", interviewId);
                    await update.ExecuteNonQueryAsync();
                }
                await InsertStatusHistoryAsync(connection, transaction, context.ApplicationId, "EvaluationSubmitted", decision, hiringManagerUserId, changedAt, $"Hiring decision: {decision}.");
                transaction.Commit();
                return decision;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static async Task<(bool Exists, int ApplicationId, string ApplicationStatus, int? InterviewerId)> GetEvaluationContextAsync(
            SqlConnection connection, SqlTransaction transaction, int interviewId, int hiringManagerUserId)
        {
            await using var command = new SqlCommand(@"
                SELECT i.ApplicationId, ja.CurrentStatus,
                       assigned.InterviewerId
                FROM Interview i
                INNER JOIN JobApplications ja WITH (UPDLOCK, HOLDLOCK) ON ja.ApplicationId = i.ApplicationId
                LEFT JOIN (
                    SELECT ia.InterviewId, interviewer.InterviewerId
                    FROM InterviewAssignment ia
                    INNER JOIN Interviewer interviewer ON interviewer.InterviewerId = ia.InterviewerId
                    WHERE interviewer.UserId = @UserId
                ) assigned ON assigned.InterviewId = i.InterviewId
                WHERE i.InterviewId = @InterviewId", connection, transaction);
            command.Parameters.AddWithValue("@UserId", hiringManagerUserId);
            command.Parameters.AddWithValue("@InterviewId", interviewId);
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return (false, 0, string.Empty, null);
            return (true, reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetInt32(2));
        }

        private static async Task InsertStatusHistoryAsync(SqlConnection connection, SqlTransaction transaction, int applicationId, string oldStatus, string newStatus, int changedBy, DateTime changedAt, string comment)
        {
            await using var command = new SqlCommand(@"
                INSERT INTO ApplicationStatusHistories (ApplicationId, OldStatus, NewStatus, ChangedBy, ChangedAt, Comment)
                VALUES (@ApplicationId, @OldStatus, @NewStatus, @ChangedBy, @ChangedAt, @Comment)", connection, transaction);
            command.Parameters.AddWithValue("@ApplicationId", applicationId);
            command.Parameters.AddWithValue("@OldStatus", oldStatus);
            command.Parameters.AddWithValue("@NewStatus", newStatus);
            command.Parameters.AddWithValue("@ChangedBy", changedBy);
            command.Parameters.AddWithValue("@ChangedAt", changedAt);
            command.Parameters.AddWithValue("@Comment", comment);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<HiringDashboardResponse> GetHiringDashboardAsync()
        {
            var response = new HiringDashboardResponse();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview", connection))
                    response.TotalInterviews = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE CONVERT(date, ScheduledDate) = CONVERT(date, GETDATE())", connection))
                    response.TodaysInterviews = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE ScheduledDate > GETDATE()", connection))
                    response.UpcomingInterviews = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE Status = 'Completed'", connection))
                    response.CompletedInterviews = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE Status = 'Cancelled'", connection))
                    response.CancelledInterviews = (int)(await cmd.ExecuteScalarAsync() ?? 0);

                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM Interview i
                    LEFT JOIN InterviewEvaluation e ON i.InterviewId = e.InterviewId
                    WHERE e.EvaluationId IS NULL AND i.Status = 'Completed'", connection))
                    response.PendingEvaluations = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            }
            return response;
        }

        public async Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    INSERT INTO InterviewAssignment (InterviewId, InterviewerId, Role) 
                    VALUES (@InterviewId, @InterviewerId, @Role)";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InterviewId", assignment.InterviewId);
                command.Parameters.AddWithValue("@InterviewerId", assignment.InterviewerId);
                command.Parameters.AddWithValue("@Role", (object?)assignment.Role ?? DBNull.Value);
                await command.ExecuteNonQueryAsync();
            }
            return assignment;
        }

        public async Task<IEnumerable<InterviewAssignment>> GetInterviewAssignmentsAsync(int interviewId)
        {
            var assignments = new List<InterviewAssignment>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM InterviewAssignment WHERE InterviewId = @InterviewId", connection);
                command.Parameters.AddWithValue("@InterviewId", interviewId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        assignments.Add(new InterviewAssignment
                        {
                            InterviewId = reader.GetInt32(reader.GetOrdinal("InterviewId")),
                            InterviewerId = reader.GetInt32(reader.GetOrdinal("InterviewerId")),
                            Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? null : reader.GetString(reader.GetOrdinal("Role"))
                        });
                    }
                }
            }
            return assignments;
        }

        public async Task<IEnumerable<Interview>> GetTodayInterviewsAsync()
        {
            var interviews = new List<Interview>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM Interview WHERE CONVERT(date, ScheduledDate) = CONVERT(date, GETDATE())", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var interview = MapInterview(reader);
                        interview.CandidateName = "Dinuri";
                        interview.Role = "Frontend Developer";
                        interviews.Add(interview);
                    }
                }
            }
            return interviews;
        }

        public async Task<IEnumerable<Interview>> GetPendingFeedbackAsync()
        {
            var interviews = new List<Interview>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM Interview WHERE ScheduledDate < GETDATE() AND Status = 'Scheduled'", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var interview = MapInterview(reader);
                        interview.CandidateName = "Bob Jones";
                        interview.Role = "DevOps Engineer";
                        interviews.Add(interview);
                    }
                }
            }
            return interviews;
        }
    }
}
