using Microsoft.EntityFrameworkCore;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Authentication Entities
        public DbSet<User> Users { get; set; } = null!;

        // Candidate Module Entities
        public DbSet<Candidate> Candidates { get; set; } = null!;
        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<CandidateSkill> CandidateSkills { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configuration for User (minimal placeholder to avoid issues)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", t => t.ExcludeFromMigrations()); // Since Users table already exists!
                entity.HasKey(e => e.UserID);
                entity.Ignore(e => e.UserId); // Ignore the duplicate wrapper property
            });

            // Candidate Configuration
            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasKey(c => c.UserId);

                entity.HasOne(c => c.User)
                    .WithOne(u => u.Candidate)
                    .HasForeignKey<Candidate>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            });

            // Resume Configuration
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.ResumeId);

                entity.HasOne(r => r.Candidate)
                    .WithMany(c => c.Resumes)
                    .HasForeignKey(r => r.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
                entity.Property(r => r.FilePath).IsRequired();
                entity.Property(r => r.UploadedDate).IsRequired();
            });

            // Skill Configuration
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(s => s.SkillId);

                entity.HasIndex(s => s.SkillName).IsUnique();
                entity.Property(s => s.SkillName).IsRequired().HasMaxLength(100);
            });

            // CandidateSkill Configuration
            modelBuilder.Entity<CandidateSkill>(entity =>
            {
                entity.HasKey(cs => new { cs.CandidateId, cs.SkillId });

                entity.HasOne(cs => cs.Candidate)
                    .WithMany(c => c.CandidateSkills)
                    .HasForeignKey(cs => cs.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cs => cs.Skill)
                    .WithMany(s => s.CandidateSkills)
                    .HasForeignKey(cs => cs.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
