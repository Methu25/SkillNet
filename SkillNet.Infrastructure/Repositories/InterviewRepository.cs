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
                OverallScore = reader.GetInt32(reader.GetOrdinal("OverallScore")),
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
