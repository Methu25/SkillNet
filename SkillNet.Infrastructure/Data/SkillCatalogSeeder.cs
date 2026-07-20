using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Data
{
    public class SkillCatalogSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SkillCatalogSeeder> _logger;

        public SkillCatalogSeeder(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<SkillCatalogSeeder> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.Skills.AnyAsync())
            {
                return;
            }

            var names = _configuration.GetSection("CandidateSkills:Seed")
                .Get<string[]>() ?? [];
            var skills = names
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => new Skill { SkillName = name, CreatedDate = DateTime.UtcNow })
                .ToList();

            if (skills.Count == 0)
            {
                _logger.LogWarning("Candidate skill catalog is empty and CandidateSkills:Seed is not configured.");
                return;
            }

            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {SkillCount} Candidate skills.", skills.Count);
        }
    }
}
