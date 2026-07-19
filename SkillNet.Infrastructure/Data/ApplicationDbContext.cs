using Microsoft.EntityFrameworkCore;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {

        // Authentication Entities
        public DbSet<User> Users { get; set; } = null!;

        // Candidate Module Entities
        public DbSet<Candidate> Candidates { get; set; } = null!;
        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<CandidateSkill> CandidateSkills { get; set; } = null!;

        // Interview / Hiring Manager Module Entities
        public DbSet<Interview> Interviews { get; set; } = null!;
        public DbSet<Interviewer> Interviewers { get; set; } = null!;
        public DbSet<InterviewAssignment> InterviewAssignments { get; set; } = null!;
        public DbSet<InterviewEvaluation> InterviewEvaluations { get; set; } = null!;
        public DbSet<InterviewFeedbackHistory> InterviewFeedbackHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configuration for User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", t => t.ExcludeFromMigrations());

                entity.HasKey(e => e.UserID);
                entity.Ignore(e => e.UserId);
            });

            // Candidate Configuration
            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasKey(c => c.UserId);

                entity.HasOne(c => c.User)
                    .WithOne(u => u.Candidate)
                    .HasForeignKey<Candidate>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // Resume Configuration
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.ResumeId);

                entity.HasOne(r => r.Candidate)
                    .WithMany(c => c.Resumes)
                    .HasForeignKey(r => r.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(r => r.FilePath)
                    .IsRequired();

                entity.Property(r => r.UploadedDate)
                    .IsRequired();
            });

            // Skill Configuration
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(s => s.SkillId);

                entity.HasIndex(s => s.SkillName)
                    .IsUnique();

                entity.Property(s => s.SkillName)
                    .IsRequired()
                    .HasMaxLength(100);
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

            // Interview Configuration
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.ToTable("Interview");

                entity.HasKey(e => e.InterviewId);

                entity.Property(e => e.InterviewType)
                    .HasMaxLength(100);

                entity.Property(e => e.Location)
                    .HasMaxLength(255);

                entity.Property(e => e.MeetingLink)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Display-only fields. These should not become DB columns.
                entity.Ignore(e => e.CandidateName);
                entity.Ignore(e => e.CandidateEmail);
                entity.Ignore(e => e.JobTitle);
                entity.Ignore(e => e.CandidateSummary);
                entity.Ignore(e => e.CandidateSkills);
                entity.Ignore(e => e.ExperienceYears);
                entity.Ignore(e => e.Role);
            });

            // Interviewer Configuration
            modelBuilder.Entity<Interviewer>(entity =>
            {
                entity.ToTable("Interviewer");

                entity.HasKey(e => e.InterviewerId);

                entity.Property(e => e.Position)
                    .HasMaxLength(100);
            });

            // InterviewAssignment Configuration
            modelBuilder.Entity<InterviewAssignment>(entity =>
            {
                entity.ToTable("InterviewAssignment");

                entity.HasKey(e => new { e.InterviewId, e.InterviewerId });

                entity.Property(e => e.Role)
                    .HasMaxLength(50);

                entity.HasOne(e => e.Interview)
                    .WithMany(e => e.InterviewAssignments)
                    .HasForeignKey(e => e.InterviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Interviewer)
                    .WithMany(e => e.InterviewAssignments)
                    .HasForeignKey(e => e.InterviewerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InterviewEvaluation Configuration
            modelBuilder.Entity<InterviewEvaluation>(entity =>
            {
                entity.ToTable("InterviewEvaluation");

                entity.HasKey(e => e.EvaluationId);

                entity.Property(e => e.Recommendation)
                    .HasMaxLength(50);

                entity.Property(e => e.Comments)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.SubmittedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Interview)
                    .WithMany(e => e.InterviewEvaluations)
                    .HasForeignKey(e => e.InterviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Interviewer)
                    .WithMany(e => e.InterviewEvaluations)
                    .HasForeignKey(e => e.InterviewerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InterviewFeedbackHistory Configuration
            modelBuilder.Entity<InterviewFeedbackHistory>(entity =>
            {
                entity.ToTable("InterviewFeedbackHistory");

                entity.HasKey(e => e.HistoryId);

                entity.Property(e => e.OldValue)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.NewValue)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.InterviewEvaluation)
                    .WithMany(e => e.FeedbackHistory)
                    .HasForeignKey(e => e.EvaluationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}