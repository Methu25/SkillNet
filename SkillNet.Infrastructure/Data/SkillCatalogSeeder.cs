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
            await SeedSkillsAsync();
            await SeedCategoriesAsync();
        }

        private async Task SeedSkillsAsync()
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

        private async Task SeedCategoriesAsync()
        {
            if (await _context.JobCategories.AnyAsync())
            {
                return;
            }

            var categories = new List<JobCategory>
            {
                new JobCategory { Name = "Information Technology", Description = "Software, hardware, and IT infrastructure roles" },
                new JobCategory { Name = "Finance & Accounting", Description = "Finance, accounting, and banking roles" },
                new JobCategory { Name = "Marketing & Sales", Description = "Marketing, advertising, and sales roles" },
                new JobCategory { Name = "Human Resources", Description = "HR, recruitment, and people operations roles" },
                new JobCategory { Name = "Engineering", Description = "Civil, mechanical, and electrical engineering roles" },
                new JobCategory { Name = "Design & Creative", Description = "UI/UX, graphic design, and creative roles" },
                new JobCategory { Name = "Healthcare", Description = "Medical, nursing, and healthcare administration roles" },
                new JobCategory { Name = "Education", Description = "Teaching, training, and academic roles" },
                new JobCategory { Name = "Operations & Logistics", Description = "Supply chain, logistics, and operations roles" },
                new JobCategory { Name = "Legal & Compliance", Description = "Legal, compliance, and regulatory roles" }
            };

            await _context.JobCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {CategoryCount} Job categories.", categories.Count);
        }
    }
}
