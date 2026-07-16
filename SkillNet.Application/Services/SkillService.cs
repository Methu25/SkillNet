using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.Domain.Entities;

namespace SkillNet.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICandidateService _candidateService;
        private readonly ICandidateNotificationService _notificationService;

        public SkillService(
            ICandidateRepository candidateRepository,
            ISkillRepository skillRepository,
            ICandidateService candidateService,
            ICandidateNotificationService notificationService)
        {
            _candidateRepository = candidateRepository;
            _skillRepository = skillRepository;
            _candidateService = candidateService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
        {
            var skills = await _skillRepository.GetAllSkillsAsync();
            return skills.Select(MapToSkillDto);
        }

        public async Task<IEnumerable<CandidateSkillDto>> GetCandidateSkillsAsync(int candidateId)
        {
            await EnsureCandidateExistsAsync(candidateId);
            var skills = await _skillRepository.GetSkillsByCandidateIdAsync(candidateId);
            return skills.Select(skill => MapToCandidateSkillDto(candidateId, skill));
        }

        public async Task<CandidateSkillDto> AddCandidateSkillAsync(
            int candidateId,
            AddCandidateSkillDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            await EnsureCandidateExistsAsync(candidateId);
            var previousCompletion = await GetCompletionAsync(candidateId);

            var skill = await _skillRepository.GetSkillByIdAsync(dto.SkillId);
            if (skill == null)
            {
                throw new KeyNotFoundException($"Skill {dto.SkillId} was not found.");
            }

            var assignedSkills = await _skillRepository.GetSkillsByCandidateIdAsync(candidateId);
            if (assignedSkills.Any(assignedSkill => assignedSkill.SkillId == dto.SkillId))
            {
                throw new InvalidOperationException(
                    $"Skill {dto.SkillId} is already assigned to candidate {candidateId}.");
            }

            var candidateSkill = new CandidateSkill
            {
                CandidateId = candidateId,
                SkillId = skill.SkillId
            };

            await _skillRepository.AssignSkillToCandidateAsync(candidateSkill);
            await NotifyCompletionChangeAsync(candidateId, previousCompletion);
            return MapToCandidateSkillDto(candidateId, skill);
        }

        private async Task<int> GetCompletionAsync(int userId)
        {
            return (await _candidateService.GetCandidateProfileAsync(userId))?
                .ProfileCompletion.CompletionPercentage ?? 0;
        }

        private async Task NotifyCompletionChangeAsync(int userId, int previousPercentage)
        {
            var currentPercentage = await GetCompletionAsync(userId);
            await _notificationService.NotifyProfileProgressAsync(
                userId, previousPercentage, currentPercentage);
        }

        public async Task<bool> RemoveCandidateSkillAsync(int candidateId, int skillId)
        {
            await EnsureCandidateExistsAsync(candidateId);

            var skill = await _skillRepository.GetSkillByIdAsync(skillId);
            if (skill == null)
            {
                throw new KeyNotFoundException($"Skill {skillId} was not found.");
            }

            var assignedSkills = await _skillRepository.GetSkillsByCandidateIdAsync(candidateId);
            if (!assignedSkills.Any(assignedSkill => assignedSkill.SkillId == skillId))
            {
                return false;
            }

            await _skillRepository.RemoveSkillFromCandidateAsync(candidateId, skillId);
            return true;
        }

        private async Task EnsureCandidateExistsAsync(int candidateId)
        {
            if (!await _candidateRepository.CandidateExistsAsync(candidateId))
            {
                throw new KeyNotFoundException($"Candidate profile {candidateId} was not found.");
            }
        }

        private static SkillDto MapToSkillDto(Skill skill)
        {
            return new SkillDto
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName
            };
        }

        private static CandidateSkillDto MapToCandidateSkillDto(int candidateId, Skill skill)
        {
            return new CandidateSkillDto
            {
                CandidateId = candidateId,
                SkillId = skill.SkillId,
                SkillName = skill.SkillName
            };
        }
    }
}
