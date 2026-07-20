using Microsoft.EntityFrameworkCore;
using SkillNet.Domain.Entities;

namespace SkillNet.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {

        // Authentication / Existing Reference Entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;

        // Candidate Module Entities
        public DbSet<Candidate> Candidates { get; set; } = null!;
        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<CandidateSkill> CandidateSkills { get; set; } = null!;

        // Job / Recruiter Module Entities
        public DbSet<RecruiterProfile> RecruiterProfiles { get; set; } = null!;
        public DbSet<JobCategory> JobCategories { get; set; } = null!;
        public DbSet<JobPost> JobPosts { get; set; } = null!;
        public DbSet<JobSkill> JobSkills { get; set; } = null!;

        // Application Module Entities
        public DbSet<JobApplication> JobApplications { get; set; } = null!;
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; } = null!;
        public DbSet<RecruiterNote> RecruiterNotes { get; set; } = null!;

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

            // Existing reference entities used by the recruiter/job module
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");
                entity.HasKey(e => e.OrganizationId);
                entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Industry).HasMaxLength(150);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.Logo).HasMaxLength(255);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ApprovalStatus).IsRequired().HasMaxLength(20).HasDefaultValue("Draft");
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.HasIndex(e => e.ApprovalStatus);
            });

            modelBuilder.Entity<RecruiterProfile>(entity =>
            {
                entity.ToTable("RecruiterProfile");
                entity.HasKey(e => e.RecruiterProfileId);
                entity.Property(e => e.Headline).HasMaxLength(200);
                entity.Property(e => e.Bio).HasColumnType("nvarchar(max)");
                entity.Property(e => e.LinkedInUrl).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.OrganizationId);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(e => e.JobPosts)
                    .WithOne(e => e.RecruiterProfile)
                    .HasForeignKey(e => e.RecruiterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Job / Recruiter Configuration
            modelBuilder.Entity<JobCategory>(entity =>
            {
                entity.ToTable("JobCategory");
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasMany(e => e.JobPosts)
                    .WithOne(e => e.JobCategory)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<JobPost>(entity =>
            {
                entity.ToTable("JobPost");
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.WorkMode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ExperienceLevel).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SalaryMin).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalaryMax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.RecruiterId);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Status);
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<JobSkill>(entity =>
            {
                entity.ToTable("JobSkill");
                entity.HasKey(e => new { e.JobId, e.SkillId });
                entity.Ignore(e => e.SkillName);
                entity.HasIndex(e => e.SkillId);
                entity.HasOne(e => e.JobPost)
                    .WithMany(e => e.JobSkills)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Skill)
                    .WithMany(e => e.JobSkills)
                    .HasForeignKey(e => e.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Application Module Configuration
            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.ToTable("JobApplications");
                entity.HasKey(e => e.ApplicationId);

                entity.Property(e => e.CandidateId)
                    .IsRequired();

                entity.Property(e => e.JobId)
                    .IsRequired();

                entity.Property(e => e.ResumeId)
                    .IsRequired();

                entity.Property(e => e.CurrentStatus)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CoverLetter)
                    .HasMaxLength(2000);

                entity.Property(e => e.Source)
                    .HasMaxLength(100);

                entity.Property(e => e.AppliedDate)
                    .IsRequired();

                entity.Property(e => e.LastUpdated)
                    .IsRequired();

                entity.HasOne(e => e.Candidate)
                    .WithMany(e => e.JobApplications)
                    .HasForeignKey(e => e.CandidateId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Job)
                    .WithMany(e => e.JobApplications)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Resume)
                    .WithMany(e => e.JobApplications)
                    .HasForeignKey(e => e.ResumeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.CandidateId, e.JobId })
                    .IsUnique();
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.CandidateId);
                entity.HasIndex(e => e.CurrentStatus);
                entity.HasIndex(e => e.AppliedDate);
            });

            modelBuilder.Entity<ApplicationStatusHistory>(entity =>
            {
                entity.ToTable("ApplicationStatusHistories");
                entity.HasKey(e => e.StatusHistoryId);

                entity.Property(e => e.ApplicationId)
                    .IsRequired();

                entity.Property(e => e.OldStatus)
                    .HasMaxLength(50);

                entity.Property(e => e.NewStatus)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ChangedBy)
                    .IsRequired();

                entity.Property(e => e.ChangedAt)
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .HasMaxLength(2000);

                entity.HasOne(e => e.Application)
                    .WithMany(e => e.StatusHistory)
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ChangedByUser)
                    .WithMany(e => e.ApplicationStatusChanges)
                    .HasForeignKey(e => e.ChangedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.ChangedAt);
            });

            modelBuilder.Entity<RecruiterNote>(entity =>
            {
                entity.ToTable("RecruiterNotes");
                entity.HasKey(e => e.NoteId);

                entity.Property(e => e.ApplicationId)
                    .IsRequired();

                entity.Property(e => e.RecruiterId)
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.HasOne(e => e.Application)
                    .WithMany(e => e.RecruiterNotes)
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recruiter)
                    .WithMany(e => e.RecruiterNotes)
                    .HasForeignKey(e => e.RecruiterId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ApplicationId);
                entity.HasIndex(e => e.RecruiterId);
                entity.HasIndex(e => e.CreatedAt);
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

                entity.HasOne(e => e.Application)
                    .WithMany(e => e.Interviews)
                    .HasForeignKey(e => e.ApplicationId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

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
