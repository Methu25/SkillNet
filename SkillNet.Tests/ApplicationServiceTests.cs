using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Application.Policies;
using SkillNet.Application.Services;
using SkillNet.Domain.Entities;
using Xunit;
namespace SkillNet.Tests
{
    public class FakeSystemConfigurationService : ISystemConfigurationService
    {
        public string GetSetting(string key, string defaultValue) => defaultValue;
        public bool GetBoolSetting(string key, bool defaultValue) => defaultValue;
        public int GetIntSetting(string key, int defaultValue) => defaultValue;
    }

    public class ApplicationServiceTests
    {
        private readonly FakeApplicationRepository _appRepo;
        private readonly FakeJobRepository _jobRepo;
        private readonly FakeResumeRepository _resumeRepo;
        private readonly FakeSkillRepository _skillRepo;
        private readonly FakeSystemConfigurationService _systemConfig;
        private readonly RequiredSkillCoverageStrategy _matchingStrategy;
        private readonly ApplicationStatusTransitionPolicy _transitionPolicy;
        private readonly ApplicationService _service;

        public ApplicationServiceTests()
        {
            _appRepo = new FakeApplicationRepository();
            _jobRepo = new FakeJobRepository();
            _resumeRepo = new FakeResumeRepository();
            _skillRepo = new FakeSkillRepository();
            _systemConfig = new FakeSystemConfigurationService();
            _matchingStrategy = new RequiredSkillCoverageStrategy();
            _transitionPolicy = new ApplicationStatusTransitionPolicy();

            _service = new ApplicationService(
                _appRepo,
                _jobRepo,
                _resumeRepo,
                _systemConfig,
                _skillRepo,
                _matchingStrategy,
                _transitionPolicy
            );
        }

        [Fact]
        public async Task GetApplicationsForJobAsync_SortsByMatchScoreDescendingThenAppliedDateDescending()
        {
            // Arrange
            int jobId = 1;
            int recruiterId = 10;

            var job = new JobPost
            {
                JobId = jobId,
                RecruiterId = recruiterId,
                Title = "Software Developer",
                Status = "Published"
            };
            _jobRepo.Jobs.Add(job);

            // Job requires skills: 1 (C#), 2 (React)
            _jobRepo.JobSkillIds[jobId] = new List<int> { 1, 2 };
            _jobRepo.JobSkillNames[jobId] = new List<string> { "C#", "React" };

            // Candidate A has 2 matches (100%): C#, React
            var candA = new Candidate { UserId = 101, FirstName = "Candidate", LastName = "A" };
            _skillRepo.CandidateSkills[101] = new List<Skill>
            {
                new() { SkillId = 1, SkillName = "C#" },
                new() { SkillId = 2, SkillName = "React" }
            };

            // Candidate B has 1 match (50%): C#
            var candB = new Candidate { UserId = 102, FirstName = "Candidate", LastName = "B" };
            _skillRepo.CandidateSkills[102] = new List<Skill>
            {
                new() { SkillId = 1, SkillName = "C#" }
            };

            // Candidate C has 1 match (50%): C# (Applied later than B)
            var candC = new Candidate { UserId = 103, FirstName = "Candidate", LastName = "C" };
            _skillRepo.CandidateSkills[103] = new List<Skill>
            {
                new() { SkillId = 1, SkillName = "C#" }
            };

            var appA = new JobApplication
            {
                ApplicationId = 1,
                JobId = jobId,
                CandidateId = 101,
                Candidate = candA,
                AppliedDate = DateTime.UtcNow.AddMinutes(-10),
                CurrentStatus = "Applied"
            };

            var appB = new JobApplication
            {
                ApplicationId = 2,
                JobId = jobId,
                CandidateId = 102,
                Candidate = candB,
                AppliedDate = DateTime.UtcNow.AddMinutes(-20), // applied earlier
                CurrentStatus = "Applied"
            };

            var appC = new JobApplication
            {
                ApplicationId = 3,
                JobId = jobId,
                CandidateId = 103,
                Candidate = candC,
                AppliedDate = DateTime.UtcNow.AddMinutes(-5), // applied later
                CurrentStatus = "Applied"
            };

            _appRepo.Applications.AddRange(new[] { appB, appA, appC });

            var request = new ApplicationSearchRequest { PageNumber = 1, PageSize = 10 };

            // Act
            var result = (await _service.GetApplicationsForJobAsync(jobId, recruiterId, request)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            // First should be App A (100% score)
            Assert.Equal(101, result[0].CandidateId);
            Assert.Equal(100, result[0].MatchScore);

            // Second should be App C (50% score, applied later/newer first)
            Assert.Equal(103, result[1].CandidateId);
            Assert.Equal(50, result[1].MatchScore);

            // Third should be App B (50% score, applied earlier/older second)
            Assert.Equal(102, result[2].CandidateId);
            Assert.Equal(50, result[2].MatchScore);
        }

        [Fact]
        public async Task GetApplicationsForJobAsync_UnauthorizedRecruiter_ThrowsInvalidOperationException()
        {
            // Arrange
            int jobId = 1;
            int ownerRecruiterId = 10;
            int otherRecruiterId = 20;

            var job = new JobPost { JobId = jobId, RecruiterId = ownerRecruiterId, Status = "Published" };
            _jobRepo.Jobs.Add(job);

            var request = new ApplicationSearchRequest { PageNumber = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetApplicationsForJobAsync(jobId, otherRecruiterId, request));
        }

        [Fact]
        public async Task GetApplicationsForJobAsync_MissingJob_ThrowsKeyNotFoundException()
        {
            // Arrange
            int missingJobId = 999;
            int recruiterId = 10;

            var request = new ApplicationSearchRequest { PageNumber = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetApplicationsForJobAsync(missingJobId, recruiterId, request));
        }

        [Fact]
        public async Task GetApplicationsForJobAsync_CandidateWithoutSkills_ReturnsZeroScore()
        {
            // Arrange
            int jobId = 1;
            int recruiterId = 10;

            var job = new JobPost { JobId = jobId, RecruiterId = recruiterId, Status = "Published" };
            _jobRepo.Jobs.Add(job);
            _jobRepo.JobSkillIds[jobId] = new List<int> { 1 };
            _jobRepo.JobSkillNames[jobId] = new List<string> { "C#" };

            var cand = new Candidate { UserId = 101, FirstName = "NoSkills", LastName = "Candidate" };
            _appRepo.Applications.Add(new JobApplication
            {
                ApplicationId = 1,
                JobId = jobId,
                CandidateId = 101,
                Candidate = cand,
                AppliedDate = DateTime.UtcNow,
                CurrentStatus = "Applied"
            });

            var request = new ApplicationSearchRequest { PageNumber = 1, PageSize = 10 };

            // Act
            var result = (await _service.GetApplicationsForJobAsync(jobId, recruiterId, request)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(0, result[0].MatchScore);
            Assert.Contains("C#", result[0].MissingSkills!);
        }

        // --- UpdateApplicationStatus tests ---

        private JobApplication MakeAppliedApplication(int appId, int recruiterId, int candidateId = 999)
        {
            var recruiterProfile = new RecruiterProfile { RecruiterProfileId = recruiterId, UserId = recruiterId + 1000 };
            var job = new JobPost { JobId = 50, RecruiterId = recruiterId, RecruiterProfile = recruiterProfile, Status = "Published" };
            var application = new JobApplication
            {
                ApplicationId = appId,
                JobId = 50,
                CandidateId = candidateId,
                CurrentStatus = "Applied",
                Job = job,
                Candidate = new Candidate { UserId = candidateId, FirstName = "Test", LastName = "Candidate" },
                Resume = new Resume { ResumeId = 1, CandidateId = candidateId },
                StatusHistory = new List<ApplicationStatusHistory>()
            };
            _jobRepo.Jobs.Add(job);
            _appRepo.Applications.Add(application);
            return application;
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_AppliedToShortlisted_Succeeds()
        {
            MakeAppliedApplication(appId: 100, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var result = await _service.UpdateApplicationStatusAsync(recruiterId: 10, applicationId: 100, dto);

            Assert.NotNull(result);
            Assert.Equal("Shortlisted", result!.CurrentStatus);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_UnknownStatus_ThrowsArgumentException()
        {
            MakeAppliedApplication(appId: 101, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "AdministratorApproved" };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateApplicationStatusAsync(10, 101, dto));
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_IllegalTransition_ThrowsInvalidOperationException()
        {
            // Applied → Hired is not a legal direct transition
            MakeAppliedApplication(appId: 102, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "Hired" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateApplicationStatusAsync(10, 102, dto));
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_TerminalStatus_ThrowsInvalidOperationException()
        {
            var application = MakeAppliedApplication(appId: 103, recruiterId: 10);
            application.CurrentStatus = "Rejected";
            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateApplicationStatusAsync(10, 103, dto));
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_SameStatus_ReturnsCurrentApplicationWithoutWriting()
        {
            MakeAppliedApplication(appId: 104, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "Applied" };

            // Should not throw; should return existing application idempotently
            var result = await _service.UpdateApplicationStatusAsync(10, 104, dto);

            Assert.NotNull(result);
            Assert.Equal("Applied", result!.CurrentStatus);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_WithdrawnBlocked_ThrowsInvalidOperationException()
        {
            MakeAppliedApplication(appId: 105, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "Withdrawn" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateApplicationStatusAsync(10, 105, dto));
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_WrongRecruiter_ReturnsNull()
        {
            MakeAppliedApplication(appId: 106, recruiterId: 10);
            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var result = await _service.UpdateApplicationStatusAsync(recruiterId: 99, applicationId: 106, dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_MissingApplication_ReturnsNull()
        {
            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var result = await _service.UpdateApplicationStatusAsync(10, 9999, dto);

            Assert.Null(result);
        }

        // --- Fakes ---

        private class FakeApplicationRepository : IApplicationRepository
        {
            public List<JobApplication> Applications { get; } = new();

            public Task<JobApplication> AddApplicationAsync(JobApplication application) => Task.FromResult(application);
            public Task<JobApplication?> GetApplicationByIdAsync(int applicationId) => Task.FromResult(Applications.FirstOrDefault(a => a.ApplicationId == applicationId));
            public Task<IEnumerable<JobApplication>> GetApplicationsByCandidateIdAsync(int candidateId) => Task.FromResult<IEnumerable<JobApplication>>(Applications.Where(a => a.CandidateId == candidateId));
            public Task<bool> HasCandidateAppliedAsync(int candidateId, int jobId) => Task.FromResult(Applications.Any(a => a.CandidateId == candidateId && a.JobId == jobId));
            public Task<bool> WithdrawApplicationAsync(JobApplication application) => Task.FromResult(true);
            public Task<IEnumerable<JobApplication>> GetApplicationsByJobIdAsync(int jobId) => Task.FromResult<IEnumerable<JobApplication>>(Applications.Where(a => a.JobId == jobId));
            public Task UpdateApplicationAsync(JobApplication application) => Task.CompletedTask;
            public Task<RecruiterNote> AddRecruiterNoteAsync(RecruiterNote recruiterNote) => Task.FromResult(recruiterNote);
            public Task<ApplicationStatusHistory> AddStatusHistoryAsync(ApplicationStatusHistory statusHistory) => Task.FromResult(statusHistory);
            public Task<Dictionary<string, int>> GetApplicationStatisticsAsync(int recruiterId, int? jobId = null) => Task.FromResult(new Dictionary<string, int>());
        }

        private class FakeJobRepository : IJobRepository
        {
            public List<JobPost> Jobs { get; } = new();
            public Dictionary<int, List<int>> JobSkillIds { get; } = new();
            public Dictionary<int, List<string>> JobSkillNames { get; } = new();

            public Task<JobPost?> GetJobByIdAsync(int jobId) => Task.FromResult(Jobs.FirstOrDefault(j => j.JobId == jobId));
            public Task<IEnumerable<int>> GetSkillIdsByJobIdAsync(int jobId) => Task.FromResult<IEnumerable<int>>(JobSkillIds.TryGetValue(jobId, out var val) ? val : new List<int>());
            public Task<IEnumerable<string>> GetSkillsByJobIdAsync(int jobId) => Task.FromResult<IEnumerable<string>>(JobSkillNames.TryGetValue(jobId, out var val) ? val : new List<string>());

            // Unused by these service tests
            public Task<int> InsertJobAsync(JobPost job) => throw new NotImplementedException();
            public Task<bool> UpdateJobAsync(JobPost job) => throw new NotImplementedException();
            public Task<bool> DeleteJobAsync(int jobId, int recruiterProfileId) => throw new NotImplementedException();
            public Task<bool> UpdateJobStatusAsync(int jobId, int recruiterProfileId, string status) => throw new NotImplementedException();
            public Task<IEnumerable<JobPost>> SearchJobsAsync(JobSearchRequest request) => throw new NotImplementedException();
            public Task<IEnumerable<JobPost>> GetJobsByRecruiterAsync(int recruiterId) => throw new NotImplementedException();
            public Task InsertJobSkillsAsync(int jobId, List<int> skillIds) => throw new NotImplementedException();
            public Task DeleteJobSkillsAsync(int jobId) => throw new NotImplementedException();
            public Task<IEnumerable<SkillDto>> GetAllSkillsAsync() => throw new NotImplementedException();
            public Task<int> GetRecruiterOrganizationIdAsync(int recruiterProfileId) => throw new NotImplementedException();
            public Task<int> InsertJobWithSkillsAsync(JobPost job, List<int> skillIds) => throw new NotImplementedException();
            public Task<bool> UpdateJobWithSkillsAsync(JobPost job, List<int> skillIds) => throw new NotImplementedException();
            public Task<IEnumerable<JobPost>> GetActiveJobsAsync() => throw new NotImplementedException();
            public Task<ILookup<int, SkillDto>> GetActiveJobSkillsAsync() => throw new NotImplementedException();
        }

        private class FakeResumeRepository : IResumeRepository
        {
            public Task<IEnumerable<Resume>> GetAllResumesByCandidateIdAsync(int candidateId) => throw new NotImplementedException();
            public Task<Resume?> GetActiveResumeByCandidateIdAsync(int candidateId) => throw new NotImplementedException();
            public Task<Resume?> GetResumeByIdAsync(int resumeId) => throw new NotImplementedException();
            public Task<IEnumerable<Resume>> GetResumesByCandidateIdAsync(int candidateId) => throw new NotImplementedException();
            public Task<Resume> AddResumeAsync(Resume resume) => throw new NotImplementedException();
            public Task UpdateResumeAsync(Resume resume) => throw new NotImplementedException();
            public Task DeleteResumeAsync(int resumeId) => throw new NotImplementedException();
        }

        private class FakeSkillRepository : ISkillRepository
        {
            public Dictionary<int, List<Skill>> CandidateSkills { get; } = new();

            public Task<IEnumerable<Skill>> GetSkillsByCandidateIdAsync(int candidateId) => Task.FromResult<IEnumerable<Skill>>(CandidateSkills.TryGetValue(candidateId, out var val) ? val : new List<Skill>());

            public Task<ILookup<int, Skill>> GetSkillsByCandidateIdsAsync(IEnumerable<int> candidateIds)
            {
                var list = new List<KeyValuePair<int, Skill>>();
                foreach (var cid in candidateIds)
                {
                    if (CandidateSkills.TryGetValue(cid, out var skills))
                    {
                        foreach (var s in skills)
                        {
                            list.Add(new KeyValuePair<int, Skill>(cid, s));
                        }
                    }
                }
                return Task.FromResult(list.ToLookup(x => x.Key, x => x.Value));
            }

            public Task<IEnumerable<Skill>> GetAllSkillsAsync() => throw new NotImplementedException();
            public Task<Skill?> GetSkillByIdAsync(int skillId) => throw new NotImplementedException();
            public Task<Skill> AddSkillAsync(Skill skill) => throw new NotImplementedException();
            public Task<CandidateSkill> AssignSkillToCandidateAsync(CandidateSkill candidateSkill) => throw new NotImplementedException();
            public Task RemoveSkillFromCandidateAsync(int candidateId, int skillId) => throw new NotImplementedException();
            public Task<bool> SkillExistsAsync(string skillName) => throw new NotImplementedException();
        }
    }
}
