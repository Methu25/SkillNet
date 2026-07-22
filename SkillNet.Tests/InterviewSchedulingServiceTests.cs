using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;

namespace SkillNet.Tests;

public class InterviewSchedulingServiceTests
{
    [Fact]
    public async Task ShortlistedOwnedApplicationSchedulesAndDeduplicatesInterviewers()
    {
        var repository = new FakeInterviewRepository();
        var service = CreateService(repository);
        var request = ValidRequest();
        request.InterviewerIds = [7, 7];

        var result = await service.CreateInterviewAsync(request);

        Assert.Equal("Scheduled", result.Status);
        Assert.Equal(DateTimeKind.Utc, result.ScheduledDate.Kind);
        Assert.Equal([7], repository.CreatedWithInterviewerIds);
        Assert.Equal(42, repository.ChangedByUserId);
        Assert.Single(result.AssignedInterviewers);
    }

    [Theory]
    [InlineData("Applied")]
    [InlineData("Interviewing")]
    [InlineData("Rejected")]
    [InlineData("Withdrawn")]
    [InlineData("Hired")]
    public async Task NonShortlistedApplicationIsRejected(string status)
    {
        var repository = new FakeInterviewRepository { Context = new() { ApplicationId = 10, RecruiterUserId = 42, CurrentStatus = status } };
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).CreateInterviewAsync(ValidRequest()));
        Assert.Contains("Shortlisted", exception.Message);
        Assert.False(repository.CreateCalled);
    }

    [Fact]
    public async Task PastScheduleIsRejected()
    {
        var request = ValidRequest();
        request.ScheduledDate = DateTime.UtcNow.AddMinutes(-1);
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task TimestampWithoutOffsetIsRejected()
    {
        var request = ValidRequest();
        request.ScheduledDate = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Unspecified);
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task InvalidInterviewTypeIsRejected()
    {
        var request = ValidRequest();
        request.InterviewType = "Technical";
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task OnlineInterviewRequiresHttpsMeetingLink()
    {
        var request = ValidRequest();
        request.MeetingLink = "http://example.test/meeting";
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task InPersonInterviewRequiresLocation()
    {
        var request = ValidRequest();
        request.InterviewType = "In-Person";
        request.Location = null;
        request.MeetingLink = null;
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task MissingInterviewerIsRejected()
    {
        var request = ValidRequest();
        request.InterviewerIds = [];
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task UnknownInterviewerIsRejected()
    {
        var request = ValidRequest();
        request.InterviewerIds = [999];
        await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateService(new()).CreateInterviewAsync(request));
    }

    [Fact]
    public async Task NonOwnerRecruiterIsRejected()
    {
        var repository = new FakeInterviewRepository { Context = new() { ApplicationId = 10, RecruiterUserId = 99, CurrentStatus = "Shortlisted" } };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService(repository).CreateInterviewAsync(ValidRequest()));
    }

    [Fact]
    public async Task CandidateRoleIsRejected()
    {
        var service = CreateService(new(), new FakeCurrentUserContext { UserId = 42, Recruiter = false });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateInterviewAsync(ValidRequest()));
    }

    [Fact]
    public async Task DuplicateActiveInterviewConflictIsPreserved()
    {
        var repository = new FakeInterviewRepository { CreateException = new InvalidOperationException("An active interview already exists.") };
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(repository).CreateInterviewAsync(ValidRequest()));
        Assert.Contains("already exists", exception.Message);
    }

    private static InterviewService CreateService(FakeInterviewRepository repository, FakeCurrentUserContext? currentUser = null) =>
        new(repository, currentUser ?? new FakeCurrentUserContext());

    private static CreateInterviewRequest ValidRequest() => new()
    {
        ApplicationId = 10,
        InterviewType = "Online",
        ScheduledDate = DateTime.UtcNow.AddDays(1),
        Duration = 60,
        MeetingLink = "https://meet.example.test/interview",
        InterviewerIds = [7]
    };

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public int? UserId { get; set; } = 42;
        public string? Email => "recruiter@example.test";
        public bool Recruiter { get; set; } = true;
        public bool IsInRole(string role) => Recruiter && role == "Recruiter";
    }

    private sealed class FakeInterviewRepository : IInterviewRepository
    {
        public InterviewSchedulingContext? Context { get; set; } = new() { ApplicationId = 10, RecruiterUserId = 42, CurrentStatus = "Shortlisted" };
        public Exception? CreateException { get; set; }
        public bool CreateCalled { get; private set; }
        public IReadOnlyCollection<int> CreatedWithInterviewerIds { get; private set; } = [];
        public int ChangedByUserId { get; private set; }
        public Task<InterviewSchedulingContext?> GetSchedulingContextAsync(int applicationId) => Task.FromResult(Context);
        public Task<IEnumerable<EligibleInterviewerResponse>> GetEligibleInterviewersAsync() => Task.FromResult<IEnumerable<EligibleInterviewerResponse>>([new() { InterviewerId = 7, Name = "Hiring Manager", Position = "Engineering Manager" }]);
        public Task<Interview> CreateScheduledInterviewAsync(Interview interview, IReadOnlyCollection<int> interviewerIds, int changedByUserId, string? note)
        {
            CreateCalled = true;
            CreatedWithInterviewerIds = interviewerIds;
            ChangedByUserId = changedByUserId;
            if (CreateException != null) throw CreateException;
            interview.InterviewId = 123;
            return Task.FromResult(interview);
        }

        public Task<IEnumerable<Interview>> GetAllInterviewsAsync() => Task.FromResult<IEnumerable<Interview>>([]);
        public Task<Interview?> GetInterviewByIdAsync(int id) => Task.FromResult<Interview?>(null);
        public Task<Interview> CreateInterviewAsync(Interview interview) => Task.FromResult(interview);
        public Task<Interview?> UpdateInterviewAsync(int id, Interview interview) => Task.FromResult<Interview?>(null);
        public Task<bool> DeleteInterviewAsync(int id) => Task.FromResult(false);
        public Task<Interview?> UpdateInterviewStatusAsync(int id, string status) => Task.FromResult<Interview?>(null);
        public Task<InterviewEvaluation> CreateEvaluationAsync(InterviewEvaluation evaluation) => Task.FromResult(evaluation);
        public Task<InterviewEvaluation?> GetEvaluationByInterviewIdAsync(int interviewId) => Task.FromResult<InterviewEvaluation?>(null);
        public Task<InterviewEvaluation?> UpdateEvaluationAsync(int interviewId, InterviewEvaluation evaluation) => Task.FromResult<InterviewEvaluation?>(null);
        public Task<IEnumerable<Interview>> GetUpcomingInterviewsAsync() => Task.FromResult<IEnumerable<Interview>>([]);
        public Task<IEnumerable<Interview>> GetAssignedInterviewsAsync(int hiringManagerUserId) => Task.FromResult<IEnumerable<Interview>>([]);
        public Task<Interview?> GetAssignedInterviewAsync(int interviewId, int hiringManagerUserId) => Task.FromResult<Interview?>(null);
        public Task<InterviewEvaluation> CreateEvaluationAndTransitionAsync(InterviewEvaluation evaluation, int hiringManagerUserId) => Task.FromResult(evaluation);
        public Task<string> RecordDecisionAsync(int interviewId, int hiringManagerUserId, string decision) => Task.FromResult(decision);
        public Task<HiringDashboardResponse> GetHiringDashboardAsync() => Task.FromResult(new HiringDashboardResponse());
        public Task<InterviewAssignment> AssignInterviewerAsync(InterviewAssignment assignment) => Task.FromResult(assignment);
        public Task<IEnumerable<InterviewAssignment>> GetInterviewAssignmentsAsync(int interviewId) => Task.FromResult<IEnumerable<InterviewAssignment>>([]);
    }
}
