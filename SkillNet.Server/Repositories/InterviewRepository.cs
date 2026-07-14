using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Server.DTOs;
using SkillNet.Server.Interfaces;
using SkillNet.Server.Models;
using System.Data;

namespace SkillNet.Server.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly string _connectionString;

        public InterviewRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private Interview MapInterview(SqlDataReader reader)
        {
            return new Interview
            {
                InterviewId = Convert.ToInt32(reader["InterviewId"]),
                ApplicationId = Convert.ToInt32(reader["ApplicationId"]),
                InterviewType = reader["InterviewType"] == DBNull.Value ? null : reader["InterviewType"].ToString(),
                InterviewRound = Convert.ToInt32(reader["InterviewRound"]),
                ScheduledDate = Convert.ToDateTime(reader["ScheduledDate"]),
                Duration = Convert.ToInt32(reader["Duration"]),
                Location = reader["Location"] == DBNull.Value ? null : reader["Location"].ToString(),
                MeetingLink = reader["MeetingLink"] == DBNull.Value ? null : reader["MeetingLink"].ToString(),
                Status = reader["Status"] == DBNull.Value ? null : reader["Status"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                CandidateName = HasColumn(reader, "CandidateName") && reader["CandidateName"] != DBNull.Value
                    ? reader["CandidateName"].ToString()!
                    : string.Empty,

                CandidateEmail = HasColumn(reader, "CandidateEmail") && reader["CandidateEmail"] != DBNull.Value
                    ? reader["CandidateEmail"].ToString()
                    : null,

                JobTitle = HasColumn(reader, "JobTitle") && reader["JobTitle"] != DBNull.Value
                    ? reader["JobTitle"].ToString()
                    : null,

                CandidateSummary = HasColumn(reader, "CandidateSummary") && reader["CandidateSummary"] != DBNull.Value
                    ? reader["CandidateSummary"].ToString()
                    : null,

                CandidateSkills = HasColumn(reader, "CandidateSkills") && reader["CandidateSkills"] != DBNull.Value
                    ? reader["CandidateSkills"].ToString()
                    : null,

                ExperienceYears = HasColumn(reader, "ExperienceYears") && reader["ExperienceYears"] != DBNull.Value
                    ? Convert.ToInt32(reader["ExperienceYears"])
                    : null
            };
        }

        private string GetInterviewWithCandidateQuery(string whereClause = "")
        {
            return $@"
                SELECT 
                    i.InterviewId,
                    i.ApplicationId,
                    i.InterviewType,
                    i.InterviewRound,
                    i.ScheduledDate,
                    i.Duration,
                    i.Location,
                    i.MeetingLink,
                    i.Status,
                    i.CreatedAt,

                    u.Name AS CandidateName,
                    u.Email AS CandidateEmail,
                    jp.Title AS JobTitle,
                    cp.Summary AS CandidateSummary,
                    cp.Skills AS CandidateSkills,
                    cp.ExperienceYears AS ExperienceYears

                FROM Interview i
                LEFT JOIN Applications a ON i.ApplicationId = a.Id
                LEFT JOIN CandidateProfiles cp ON a.CandidateProfileId = cp.Id
                LEFT JOIN Users u ON cp.UserId = u.Id
                LEFT JOIN JobPostings jp ON a.JobPostingId = jp.Id
                {whereClause}";
        }

        public async Task<IEnumerable<Interview>> GetAllInterviewsAsync()
        {
            var interviews = new List<Interview>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    GetInterviewWithCandidateQuery("ORDER BY i.ScheduledDate DESC"),
                    connection
                );

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        interviews.Add(MapInterview(reader));
                    }
                }
            }

            return interviews;
        }

        public async Task<Interview?> GetInterviewByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    GetInterviewWithCandidateQuery("WHERE i.InterviewId = @InterviewId"),
                    connection
                );

                command.Parameters.AddWithValue("@InterviewId", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapInterview(reader);
                    }
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
                    (
                        ApplicationId,
                        InterviewType,
                        InterviewRound,
                        ScheduledDate,
                        Duration,
                        Location,
                        MeetingLink,
                        Status,
                        CreatedAt
                    ) 
                    OUTPUT INSERTED.InterviewId
                    VALUES 
                    (
                        @ApplicationId,
                        @InterviewType,
                        @InterviewRound,
                        @ScheduledDate,
                        @Duration,
                        @Location,
                        @MeetingLink,
                        @Status,
                        @CreatedAt
                    )";

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                command.Parameters.AddWithValue("@InterviewType", (object?)interview.InterviewType ?? DBNull.Value);
                command.Parameters.AddWithValue("@InterviewRound", interview.InterviewRound);
                command.Parameters.AddWithValue("@ScheduledDate", interview.ScheduledDate);
                command.Parameters.AddWithValue("@Duration", interview.Duration);
                command.Parameters.AddWithValue("@Location", (object?)interview.Location ?? DBNull.Value);
                command.Parameters.AddWithValue("@MeetingLink", (object?)interview.MeetingLink ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)interview.Status ?? "Scheduled");
                command.Parameters.AddWithValue("@CreatedAt", interview.CreatedAt);

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("Failed to create the interview.");
                }

                interview.InterviewId = Convert.ToInt32(result);
            }

            return await GetInterviewByIdAsync(interview.InterviewId) ?? interview;
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
                    WHERE InterviewId = @InterviewId";

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@ApplicationId", interview.ApplicationId);
                command.Parameters.AddWithValue("@InterviewType", (object?)interview.InterviewType ?? DBNull.Value);
                command.Parameters.AddWithValue("@InterviewRound", interview.InterviewRound);
                command.Parameters.AddWithValue("@ScheduledDate", interview.ScheduledDate);
                command.Parameters.AddWithValue("@Duration", interview.Duration);
                command.Parameters.AddWithValue("@Location", (object?)interview.Location ?? DBNull.Value);
                command.Parameters.AddWithValue("@MeetingLink", (object?)interview.MeetingLink ?? DBNull.Value);
                command.Parameters.AddWithValue("@Status", (object?)interview.Status ?? DBNull.Value);
                command.Parameters.AddWithValue("@InterviewId", id);

                var rows = await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return null;
                }
            }

            return await GetInterviewByIdAsync(id);
        }

        public async Task<bool> DeleteInterviewAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    "DELETE FROM Interview WHERE InterviewId = @InterviewId",
                    connection
                );

                command.Parameters.AddWithValue("@InterviewId", id);

                var rows = await command.ExecuteNonQueryAsync();

                return rows > 0;
            }
        }

        public async Task<Interview?> UpdateInterviewStatusAsync(int id, string status)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    "UPDATE Interview SET Status = @Status WHERE InterviewId = @InterviewId",
                    connection
                );

                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@InterviewId", id);

                var rows = await command.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return null;
                }
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
                OverallScore = reader.GetInt32(reader.GetOrdinal("OverallScore")),
                Recommendation = reader.IsDBNull(reader.GetOrdinal("Recommendation"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Recommendation")),
                Comments = reader.IsDBNull(reader.GetOrdinal("Comments"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Comments")),
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
                    (
                        InterviewId,
                        InterviewerId,
                        TechnicalScore,
                        CommunicationScore,
                        ProblemSolvingScore,
                        CultureFitScore,
                        OverallScore,
                        Recommendation,
                        Comments,
                        SubmittedAt
                    ) 
                    OUTPUT INSERTED.EvaluationId
                    VALUES 
                    (
                        @InterviewId,
                        @InterviewerId,
                        @TechnicalScore,
                        @CommunicationScore,
                        @ProblemSolvingScore,
                        @CultureFitScore,
                        @OverallScore,
                        @Recommendation,
                        @Comments,
                        @SubmittedAt
                    )";

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

                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("Failed to create interview evaluation.");
                }

                evaluation.EvaluationId = Convert.ToInt32(result);
            }

            return evaluation;
        }

        public async Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    "SELECT * FROM InterviewEvaluation WHERE InterviewId = @InterviewId",
                    connection
                );

                command.Parameters.AddWithValue("@InterviewId", interviewId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapEvaluation(reader);
                    }
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

                var oldEvaluation = await GetEvaluationByInterviewIdAsync(interviewId);

                if (oldEvaluation != null)
                {
                    var historyQuery = @"
                        INSERT INTO InterviewFeedbackHistory
                        (
                            EvaluationId,
                            UpdatedBy,
                            OldValue,
                            NewValue,
                            UpdatedAt
                        )
                        VALUES
                        (
                            @EvaluationId,
                            @UpdatedBy,
                            @OldValue,
                            @NewValue,
                            @UpdatedAt
                        )";

                    var historyCommand = new SqlCommand(historyQuery, connection);

                    historyCommand.Parameters.AddWithValue("@EvaluationId", oldEvaluation.EvaluationId);
                    historyCommand.Parameters.AddWithValue("@UpdatedBy", evaluation.InterviewerId);
                    historyCommand.Parameters.AddWithValue(
                        "@OldValue",
                        $"Technical: {oldEvaluation.TechnicalScore}, Communication: {oldEvaluation.CommunicationScore}, ProblemSolving: {oldEvaluation.ProblemSolvingScore}, CultureFit: {oldEvaluation.CultureFitScore}, Recommendation: {oldEvaluation.Recommendation}"
                    );
                    historyCommand.Parameters.AddWithValue(
                        "@NewValue",
                        $"Technical: {evaluation.TechnicalScore}, Communication: {evaluation.CommunicationScore}, ProblemSolving: {evaluation.ProblemSolvingScore}, CultureFit: {evaluation.CultureFitScore}, Recommendation: {evaluation.Recommendation}"
                    );
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

                if (rows == 0)
                {
                    return null;
                }
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

                var command = new SqlCommand(
                    GetInterviewWithCandidateQuery("WHERE i.ScheduledDate >= @Now ORDER BY i.ScheduledDate ASC"),
                    connection
                );

                command.Parameters.AddWithValue("@Now", DateTime.Now);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        interviews.Add(MapInterview(reader));
                    }
                }
            }

            return interviews;
        }

        public async Task<IEnumerable<Interview>> GetTodayInterviewsAsync()
        {
            var interviews = new List<Interview>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(
                    GetInterviewWithCandidateQuery("WHERE CONVERT(date, i.ScheduledDate) = CONVERT(date, GETDATE()) ORDER BY i.ScheduledDate ASC"),
                    connection
                );

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        interviews.Add(MapInterview(reader));
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

                var command = new SqlCommand(
                    GetInterviewWithCandidateQuery("WHERE i.ScheduledDate < GETDATE() AND i.Status = 'Scheduled' ORDER BY i.ScheduledDate ASC"),
                    connection
                );

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        interviews.Add(MapInterview(reader));
                    }
                }
            }

            return interviews;
        }

        public async Task<HiringDashboardResponse> GetHiringDashboardAsync()
        {
            var response = new HiringDashboardResponse();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview", connection))
                {
                    response.TotalInterviews = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE CONVERT(date, ScheduledDate) = CONVERT(date, GETDATE())", connection))
                {
                    response.TodaysInterviews = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE ScheduledDate > GETDATE()", connection))
                {
                    response.UpcomingInterviews = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE Status = 'Completed'", connection))
                {
                    response.CompletedInterviews = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Interview WHERE Status = 'Cancelled'", connection))
                {
                    response.CancelledInterviews = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM Interview i
                    LEFT JOIN InterviewEvaluation e ON i.InterviewId = e.InterviewId
                    WHERE e.EvaluationId IS NULL AND i.Status = 'Completed'", connection))
                {
                    response.PendingEvaluations = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }

            return response;
        }

        public async Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO InterviewAssignment
                    (
                        InterviewId,
                        InterviewerId,
                        Role
                    )
                    VALUES
                    (
                        @InterviewId,
                        @InterviewerId,
                        @Role
                    )";

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

                var command = new SqlCommand(
                    "SELECT * FROM InterviewAssignment WHERE InterviewId = @InterviewId",
                    connection
                );

                command.Parameters.AddWithValue("@InterviewId", interviewId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        assignments.Add(new InterviewAssignment
                        {
                            InterviewId = reader.GetInt32(reader.GetOrdinal("InterviewId")),
                            InterviewerId = reader.GetInt32(reader.GetOrdinal("InterviewerId")),
                            Role = reader.IsDBNull(reader.GetOrdinal("Role"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Role"))
                        });
                    }
                }
            }

            return assignments;
        }
    }
}