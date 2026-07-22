using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;

namespace SkillNet.Tests;

public class HiringManagerWorkflowTests
{
    [Fact]
    public async Task AssignedHiringManagerSeesOnlyRepositoryAssignedInterviews()
    {
        var repository = new FakeRepository { AssignedInterviews = [new Interview { InterviewId = 5, ApplicationStatus = "Interviewing" }] };
        var result = await CreateService(repository).GetAssignedInterviewsAsync();
        Assert.Equal(5, Assert.Single(result).InterviewId);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public async Task UnassignedInterviewDetailsAreNotReturned()
    {
        var result = await CreateService(new FakeRepository()).GetAssignedInterviewAsync(99);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Candidate")]
    [InlineData("Recruiter")]
    public async Task NonHiringManagerCannotEvaluate(string role)
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService(new(), role).CreateEvaluationAsync(5, ValidEvaluation()));
    }

    [Fact]
    public async Task ValidEvaluationUsesCurrentUserAndCalculatesAverage()
    {
        var repository = new FakeRepository();
        var result = await CreateService(repository).CreateEvaluationAsync(5, ValidEvaluation());
        Assert.Equal(8.25m, result.OverallScore);
        Assert.Equal(42, repository.RequestedUserId);
        Assert.True(repository.EvaluationTransitionCalled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task OutOfRangeScoreIsRejected(int score)
    {
        var request = ValidEvaluation();
        request.TechnicalScore = score;
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateEvaluationAsync(5, request));
    }

    [Fact]
    public async Task InvalidRecommendationIsRejected()
    {
        var request = ValidEvaluation();
        request.Recommendation = "Hold";
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateEvaluationAsync(5, request));
    }

    [Fact]
    public async Task MissingCommentsAreRejected()
    {
        var request = ValidEvaluation();
        request.Comments = " ";
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateEvaluationAsync(5, request));
    }

    [Fact]
    public async Task DuplicateEvaluationConflictIsPreserved()
    {
        var repository = new FakeRepository { EvaluationException = new InvalidOperationException("already submitted") };
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).CreateEvaluationAsync(5, ValidEvaluation()));
    }

    [Fact]
    public async Task DecisionWithoutEvaluationConflictIsPreserved()
    {
        var repository = new FakeRepository { DecisionException = new InvalidOperationException("evaluation must be submitted") };
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).RecordDecisionAsync(5, new() { Decision = "Hired" }));
    }

    [Theory]
    [InlineData("Hired")]
    [InlineData("Rejected")]
    public async Task ValidFinalDecisionSucceeds(string decision)
    {
        var repository = new FakeRepository();
        var result = await CreateService(repository).RecordDecisionAsync(5, new() { Decision = decision });
        Assert.Equal(decision, result);
        Assert.Equal(42, repository.RequestedUserId);
    }

    [Fact]
    public async Task InvalidFinalDecisionIsRejectedBeforePersistence()
    {
        var repository = new FakeRepository();
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(repository).RecordDecisionAsync(5, new() { Decision = "Interviewing" }));
        Assert.False(repository.DecisionCalled);
    }

    [Fact]
    public async Task TerminalDecisionReversalConflictIsPreserved()
    {
        var repository = new FakeRepository { DecisionException = new InvalidOperationException("cannot be reversed") };
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).RecordDecisionAsync(5, new() { Decision = "Rejected" }));
    }

    private static CreateEvaluationRequest ValidEvaluation() => new()
    {
        TechnicalScore = 9,
        CommunicationScore = 8,
        ProblemSolvingScore = 9,
        CultureFitScore = 7,
        Recommendation = "Hire",
        Comments = "Strong evidence across the assessed competencies."
    };

    private static InterviewService CreateService(FakeRepository repository, string role = "HiringManager") =>
        new(repository, new FakeCurrentUserContext(role));

    private sealed class FakeCurrentUserContext(string role) : ICurrentUserContext
    {
        public int? UserId => 42;
        public string? Email => "manager@example.test";
        public bool IsInRole(string requestedRole) => requestedRole == role;
    }

    private sealed class FakeRepository : IInterviewRepository
    {
        public IEnumerable<Interview> AssignedInterviews { get; set; } = [];
        public Exception? EvaluationException { get; set; }
        public Exception? DecisionException { get; set; }
        public int RequestedUserId { get; private set; }
        public bool EvaluationTransitionCalled { get; private set; }
        public bool DecisionCalled { get; private set; }

        public Task<IEnumerable<Interview>> GetAssignedInterviewsAsync(int hiringManagerUserId) { RequestedUserId = hiringManagerUserId; return Task.FromResult(AssignedInterviews); }
        public Task<Interview?> GetAssignedInterviewAsync(int interviewId, int hiringManagerUserId) { RequestedUserId = hiringManagerUserId; return Task.FromResult(AssignedInterviews.SingleOrDefault(item => item.InterviewId == interviewId)); }
        public Task<InterviewEvaluation> CreateEvaluationAndTransitionAsync(InterviewEvaluation evaluation, int hiringManagerUserId)
        {
            RequestedUserId = hiringManagerUserId;
            EvaluationTransitionCalled = true;
            if (EvaluationException != null) throw EvaluationException;
            evaluation.EvaluationId = 10;
            evaluation.InterviewerId = 3;
            return Task.FromResult(evaluation);
        }
        public Task<string> RecordDecisionAsync(int interviewId, int hiringManagerUserId, string decision)
        {
            RequestedUserId = hiringManagerUserId;
            DecisionCalled = true;
            if (DecisionException != null) throw DecisionException;
            return Task.FromResult(decision);
        }

        public Task<IEnumerable<Interview>> GetAllInterviewsAsync() => Task.FromResult<IEnumerable<Interview>>([]);
        public Task<Interview?> GetInterviewByIdAsync(int id) => Task.FromResult<Interview?>(null);
        public Task<Interview> CreateInterviewAsync(Interview interview) => Task.FromResult(interview);
        public Task<InterviewSchedulingContext?> GetSchedulingContextAsync(int applicationId) => Task.FromResult<InterviewSchedulingContext?>(null);
        public Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync() => Task.FromResult<IEnumerable<EligibleInterviewerResponse>>([]);
        public Task<Interview> CreateScheduledInterviewAsync(Interview interview, IReadOnlyCollection<int> interviewerIds, int changedByUserId, string? note) => Task.FromResult(interview);
        public Task<Interview?> UpdateInterviewAsync(int id, Interview interview) => Task.FromResult<Interview?>(null);
        public Task<bool> DeleteInterviewAsync(int id) => Task.FromResult(false);
        public Task<Interview?> UpdateInterviewStatusAsync(int id, string status) => Task.FromResult<Interview?>(null);
        public Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation) => Task.FromResult(evaluation);
        public Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId) => Task.FromResult<InterviewEvaluation?>(null);
        public Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation) => Task.FromResult<InterviewEvaluation?>(null);
        public Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync() => Task.FromResult<IEnumerable<Interview>>([]);
        public Task<HiringDashboardResponse> GetHiringDashboardAsync() => Task.FromResult(new HiringDashboardResponse());
        public Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment) => Task.FromResult(assignment);
        public Task<IEnumerable<InterviewAssignment>> GetInterviewAssignmentsAsync(int interviewId) => Task.FromResult<IEnumerable<InterviewAssignment>>([]);
    }
}
