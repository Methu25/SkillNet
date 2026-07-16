using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IProfileCompletionStrategy _profileCompletionStrategy;
        private readonly ICandidateProfileBuilder _candidateProfileBuilder;

        public CandidateService(
            ICandidateRepository candidateRepository,
            IProfileCompletionStrategy profileCompletionStrategy,
            ICandidateProfileBuilder candidateProfileBuilder)
        {
            _candidateRepository = candidateRepository;
            _profileCompletionStrategy = profileCompletionStrategy;
            _candidateProfileBuilder = candidateProfileBuilder;
        }

        public async Task<CandidateProfileDto> CreateCandidateAsync(int userId, CreateCandidateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidateRequiredNames(dto.FirstName, dto.LastName);

            if (await _candidateRepository.CandidateExistsAsync(userId))
            {
                throw new InvalidOperationException($"A candidate profile already exists for user {userId}.");
            }

            var candidate = _candidateProfileBuilder
                .SetBasicInformation(dto.FirstName, dto.LastName, dto.PhoneNumber, dto.Location)
                .SetProfessionalInformation(dto.ProfessionalTitle, dto.ProfessionalSummary)
                .SetEducation(dto.Education, dto.Degree, dto.University)
                .SetExperience(dto.ExperienceYears)
                .Build(userId);

            var profile = MapToProfileDto(candidate);
            profile.ProfileCompletion = await _profileCompletionStrategy.CalculateAsync(profile);
            candidate.IsProfileCompleted = profile.ProfileCompletion.IsComplete;

            var createdCandidate = await _candidateRepository.AddCandidateAsync(candidate);
            return await MapToCompletedProfileDtoAsync(createdCandidate);
        }

        public async Task<CandidateProfileDto?> GetCandidateProfileAsync(int userId)
        {
            var candidate = await _candidateRepository.GetCandidateByUserIdAsync(userId);
            return candidate == null ? null : await MapToCompletedProfileDtoAsync(candidate);
        }

        public async Task<CandidateProfileSummaryDto?> GetCandidateProfileSummaryAsync(int userId)
        {
            var candidate = await _candidateRepository.GetCandidateByUserIdAsync(userId);
            return candidate == null ? null : MapToProfileSummaryDto(candidate);
        }

        public async Task<CandidateProfileDto?> UpdateCandidateAsync(int userId, UpdateCandidateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ValidateRequiredNames(dto.FirstName, dto.LastName);

            var candidate = await _candidateRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null)
            {
                return null;
            }

            candidate.FirstName = dto.FirstName.Trim();
            candidate.LastName = dto.LastName.Trim();
            candidate.PhoneNumber = dto.PhoneNumber;
            candidate.Location = dto.Location;
            candidate.ProfessionalTitle = dto.ProfessionalTitle;
            candidate.ProfessionalSummary = dto.ProfessionalSummary;
            candidate.Education = dto.Education;
            candidate.Degree = dto.Degree;
            candidate.University = dto.University;
            candidate.ExperienceYears = dto.ExperienceYears;
            candidate.ProfileImagePath = dto.ProfileImagePath;
            candidate.UpdatedDate = DateTime.UtcNow;

            var profile = MapToProfileDto(candidate);
            profile.ProfileCompletion = await _profileCompletionStrategy.CalculateAsync(profile);
            candidate.IsProfileCompleted = profile.ProfileCompletion.IsComplete;

            await _candidateRepository.UpdateCandidateAsync(candidate);
            return await MapToCompletedProfileDtoAsync(candidate);
        }

        public async Task<bool> DeleteCandidateAsync(int userId)
        {
            if (!await _candidateRepository.CandidateExistsAsync(userId))
            {
                return false;
            }

            await _candidateRepository.DeleteCandidateAsync(userId);
            return true;
        }

        private async Task<CandidateProfileDto> MapToCompletedProfileDtoAsync(Candidate candidate)
        {
            var profile = MapToProfileDto(candidate);
            profile.ProfileCompletion = await _profileCompletionStrategy.CalculateAsync(profile);
            profile.IsProfileCompleted = profile.ProfileCompletion.IsComplete;
            return profile;
        }

        private static CandidateProfileDto MapToProfileDto(Candidate candidate)
        {
            return new CandidateProfileDto
            {
                UserId = candidate.UserId,
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                PhoneNumber = candidate.PhoneNumber,
                Location = candidate.Location,
                ProfessionalTitle = candidate.ProfessionalTitle,
                ProfessionalSummary = candidate.ProfessionalSummary,
                Education = candidate.Education,
                Degree = candidate.Degree,
                University = candidate.University,
                ExperienceYears = candidate.ExperienceYears,
                ProfileImagePath = candidate.ProfileImagePath,
                CreatedDate = candidate.CreatedDate,
                UpdatedDate = candidate.UpdatedDate,
                IsProfileCompleted = candidate.IsProfileCompleted,
                ActiveResume = candidate.Resumes
                    .Where(resume => resume.IsActive)
                    .OrderByDescending(resume => resume.UploadedDate)
                    .Select(MapToResumeDto)
                    .FirstOrDefault(),
                Skills = candidate.CandidateSkills.Select(MapToCandidateSkillDto).ToList()
            };
        }

        private static CandidateProfileSummaryDto MapToProfileSummaryDto(Candidate candidate)
        {
            return new CandidateProfileSummaryDto
            {
                UserId = candidate.UserId,
                FullName = $"{candidate.FirstName} {candidate.LastName}".Trim(),
                ProfessionalTitle = candidate.ProfessionalTitle,
                ProfessionalSummary = candidate.ProfessionalSummary,
                Education = candidate.Education,
                Degree = candidate.Degree,
                Location = candidate.Location,
                ExperienceYears = candidate.ExperienceYears,
                ProfileImagePath = candidate.ProfileImagePath,
                IsProfileCompleted = candidate.IsProfileCompleted
            };
        }

        private static ResumeDto MapToResumeDto(Resume resume)
        {
            return new ResumeDto
            {
                ResumeId = resume.ResumeId,
                CandidateId = resume.CandidateId,
                FileName = resume.FileName,
                FilePath = resume.FilePath,
                FileType = resume.FileType,
                FileSize = resume.FileSize,
                UploadedDate = resume.UploadedDate,
                IsActive = resume.IsActive
            };
        }

        private static CandidateSkillDto MapToCandidateSkillDto(CandidateSkill candidateSkill)
        {
            return new CandidateSkillDto
            {
                CandidateId = candidateSkill.CandidateId,
                SkillId = candidateSkill.SkillId,
                SkillName = candidateSkill.Skill.SkillName
            };
        }

        private static void ValidateRequiredNames(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name is required.", nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name is required.", nameof(lastName));
            }
        }
    }
}
