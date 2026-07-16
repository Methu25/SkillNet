using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;

namespace SkillNet.Application.Services
{
    public class BasicProfileCompletionStrategy : IProfileCompletionStrategy
    {
        public Task<ProfileCompletionResultDto> CalculateAsync(CandidateProfileDto profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            var sections = new (string Name, int Weight, bool IsComplete)[]
            {
                ("Basic information", 20,
                    HasValue(profile.FirstName) && HasValue(profile.LastName)),
                ("Professional information", 15,
                    HasValue(profile.ProfessionalTitle) && HasValue(profile.ProfessionalSummary)),
                ("Education", 15,
                    HasValue(profile.Education) && HasValue(profile.Degree) && HasValue(profile.University)),
                ("Experience", 10,
                    profile.ExperienceYears.HasValue && profile.ExperienceYears.Value > 0),
                ("Skills", 15, profile.Skills.Count > 0),
                ("Resume", 15, profile.ActiveResume != null),
                ("Profile image", 10, HasValue(profile.ProfileImagePath))
            };

            var percentage = sections.Where(section => section.IsComplete).Sum(section => section.Weight);

            return Task.FromResult(new ProfileCompletionResultDto
            {
                CompletionPercentage = percentage,
                CompletionLevel = GetCompletionLevel(percentage),
                IsComplete = percentage == 100,
                CompletedSections = sections
                    .Where(section => section.IsComplete)
                    .Select(section => section.Name)
                    .ToList(),
                MissingSections = sections
                    .Where(section => !section.IsComplete)
                    .Select(section => section.Name)
                    .ToList()
            });
        }

        private static bool HasValue(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static int GetCompletionLevel(int percentage)
        {
            return percentage switch
            {
                0 => 0,
                <= 25 => 1,
                <= 50 => 2,
                <= 75 => 3,
                < 100 => 4,
                _ => 5
            };
        }
    }
}
