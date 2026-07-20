-- =======================================================
-- SkillNet Database Initialization Script
-- Combines Auth/Security & Admin modules setup
-- =======================================================

USE master;
GO

-- Create database if it does not exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SkillNetDB')
BEGIN
    CREATE DATABASE SkillNetDB;
END
GO

USE SkillNetDB;
GO

-- ==========================================
-- 1. Drop existing tables (reverse dependency order)
-- ==========================================
-- Job & Recruiter Module (depends on nothing above)
DROP TABLE IF EXISTS JobSkill;
DROP TABLE IF EXISTS JobPost;
DROP TABLE IF EXISTS RecruiterProfile;
DROP TABLE IF EXISTS JobCategory;
DROP TABLE IF EXISTS Skill;
-- Interview Module (depends on Users)
DROP TABLE IF EXISTS InterviewFeedbackHistory;
DROP TABLE IF EXISTS InterviewEvaluation;
DROP TABLE IF EXISTS InterviewAssignment;
DROP TABLE IF EXISTS Interviewer;
DROP TABLE IF EXISTS Interview;
-- Auth & Admin Module
DROP TABLE IF EXISTS SystemConfiguration;
DROP TABLE IF EXISTS AuditLog;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS Organization;
DROP TABLE IF EXISTS RefreshTokens;
DROP TABLE IF EXISTS UserRole;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Roles;
GO

-- ==========================================
-- 2. Create tables
-- ==========================================

-- Roles Table
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

-- Users Table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Phone VARCHAR(20) NULL,
    Status VARCHAR(20) DEFAULT 'Active',
    FailedLoginAttempts INT DEFAULT 0,
    LockoutEnd DATETIME NULL,
    ResetToken VARCHAR(255) NULL,
    ResetTokenExpiry DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- UserRole Junction Table
CREATE TABLE UserRole (
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, RoleID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE
);

-- RefreshTokens Table
CREATE TABLE RefreshTokens (
    TokenID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Token VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Organization Table
CREATE TABLE Organization (
    OrganizationId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationName NVARCHAR(255) NOT NULL UNIQUE,
    Industry NVARCHAR(100) NULL,
    Website NVARCHAR(255) NULL,
    Logo NVARCHAR(255) NULL,
    Address NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Department Table
CREATE TABLE Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    DepartmentName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(OrganizationId)
);

-- AuditLog Table
CREATE TABLE AuditLog (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    Entity NVARCHAR(100) NULL,
    EntityId INT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    IPAddress NVARCHAR(50) NULL
);

-- SystemConfiguration Table
CREATE TABLE SystemConfiguration (
    [Key] NVARCHAR(100) PRIMARY KEY,
    [Value] NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL
);
GO

-- ==========================================
-- Job & Recruiter Module Tables
-- ==========================================

-- JobCategory Table (used by Singleton cache in JobCategoryService)
CREATE TABLE JobCategory (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);

-- Skill Table (master skills list)
CREATE TABLE Skill (
    SkillId INT IDENTITY(1,1) PRIMARY KEY,
    SkillName NVARCHAR(100) NOT NULL UNIQUE
);

-- RecruiterProfile Table (extends Users for Recruiter role)
CREATE TABLE RecruiterProfile (
    RecruiterProfileId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    Headline NVARCHAR(255) NULL,
    Bio NVARCHAR(MAX) NULL,
    LinkedInUrl NVARCHAR(500) NULL,
    ExperienceYears INT NULL,
    OrganizationId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (OrganizationId) REFERENCES Organization(OrganizationId)
);

-- JobPost Table (core entity — Builder Pattern assembles it, Prototype Pattern clones it)
CREATE TABLE JobPost (
    JobId INT IDENTITY(1,1) PRIMARY KEY,
    RecruiterId INT NOT NULL,
    OrganizationId INT NULL,
    CategoryId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EmploymentType NVARCHAR(50) NOT NULL,       -- Full-time, Part-time, Contract, Internship
    WorkMode NVARCHAR(50) NOT NULL,             -- Remote, Hybrid, Onsite
    Location NVARCHAR(255) NULL,
    SalaryMin DECIMAL(10,2) NULL,
    SalaryMax DECIMAL(10,2) NULL,
    ExperienceLevel NVARCHAR(50) NULL,          -- Junior, Mid, Senior
    Status NVARCHAR(50) DEFAULT 'Draft',        -- Draft, Published, Closed
    ApplicationDeadline DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RecruiterId) REFERENCES Users(UserId),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(OrganizationId),
    FOREIGN KEY (CategoryId) REFERENCES JobCategory(CategoryId)
);

-- JobSkill Junction Table (many jobs <-> many skills)
CREATE TABLE JobSkill (
    JobId INT NOT NULL,
    SkillId INT NOT NULL,
    PRIMARY KEY (JobId, SkillId),
    FOREIGN KEY (JobId) REFERENCES JobPost(JobId) ON DELETE CASCADE,
    FOREIGN KEY (SkillId) REFERENCES Skill(SkillId) ON DELETE CASCADE
);
GO

-- ==========================================
-- Interview & Hiring Manager Module Tables
-- ==========================================

-- Interview Table
CREATE TABLE Interview (
    InterviewId INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL,
    InterviewType NVARCHAR(100) NULL,
    InterviewRound INT NOT NULL,
    ScheduledDate DATETIME NOT NULL,
    Duration INT NOT NULL,             -- duration in minutes
    Location NVARCHAR(255) NULL,
    MeetingLink NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NULL,          -- Scheduled, Completed, Cancelled, Pending Feedback
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Interviewer Table (links a User to interviewer role)
CREATE TABLE Interviewer (
    InterviewerId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DepartmentId INT NULL,
    Position NVARCHAR(100) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId)
);

-- InterviewAssignment Table (many interviews <-> many interviewers)
CREATE TABLE InterviewAssignment (
    InterviewId INT NOT NULL,
    InterviewerId INT NOT NULL,
    Role NVARCHAR(50) NULL,
    PRIMARY KEY (InterviewId, InterviewerId),
    FOREIGN KEY (InterviewId) REFERENCES Interview(InterviewId) ON DELETE CASCADE,
    FOREIGN KEY (InterviewerId) REFERENCES Interviewer(InterviewerId)
);

-- InterviewEvaluation Table
CREATE TABLE InterviewEvaluation (
    EvaluationId INT IDENTITY(1,1) PRIMARY KEY,
    InterviewId INT NOT NULL,
    InterviewerId INT NOT NULL,
    TechnicalScore INT NOT NULL,
    CommunicationScore INT NOT NULL,
    ProblemSolvingScore INT NOT NULL,
    CultureFitScore INT NOT NULL,
    OverallScore INT NOT NULL,
    Recommendation NVARCHAR(50) NULL,  -- Hire, Reject, Hold
    Comments NVARCHAR(MAX) NULL,
    SubmittedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InterviewId) REFERENCES Interview(InterviewId),
    FOREIGN KEY (InterviewerId) REFERENCES Interviewer(InterviewerId)
);

-- InterviewFeedbackHistory Table (audit trail for evaluation changes)
CREATE TABLE InterviewFeedbackHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    EvaluationId INT NOT NULL,
    UpdatedBy INT NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EvaluationId) REFERENCES InterviewEvaluation(EvaluationId)
);
GO

-- ==========================================
-- 3. Seed Default Data
-- ==========================================

-- Insert the mandatory assignment roles
INSERT INTO Roles (RoleName) VALUES ('Candidate'), ('Recruiter'), ('HiringManager'), ('Admin');

-- Insert Default System Configurations
INSERT INTO SystemConfiguration ([Key], [Value], Description)
VALUES 
    ('ResumeMaxSize', '5MB', 'Maximum allowed file size for candidate resume uploads'),
    ('InterviewReminderHours', '24', 'Send automated interview reminders X hours before'),
    ('AllowMultipleApplications', 'False', 'Allow candidates to apply for multiple roles simultaneously');
GO

-- ==========================================
-- 4. Seed Job Module Default Data
-- ==========================================

-- Seed Job Categories (cached by Singleton in JobCategoryService)
INSERT INTO JobCategory (Name, Description) VALUES
    ('Information Technology', 'Software, hardware, and IT infrastructure roles'),
    ('Finance & Accounting', 'Finance, accounting, and banking roles'),
    ('Marketing & Sales', 'Marketing, advertising, and sales roles'),
    ('Human Resources', 'HR, recruitment, and people operations roles'),
    ('Engineering', 'Civil, mechanical, and electrical engineering roles'),
    ('Design & Creative', 'UI/UX, graphic design, and creative roles'),
    ('Healthcare', 'Medical, nursing, and healthcare administration roles'),
    ('Education', 'Teaching, training, and academic roles'),
    ('Operations & Logistics', 'Supply chain, logistics, and operations roles'),
    ('Legal & Compliance', 'Legal, compliance, and regulatory roles');

-- Seed Common Skills
INSERT INTO Skill (SkillName) VALUES
    ('JavaScript'), ('TypeScript'), ('React'), ('Node.js'), ('Python'),
    ('C#'), ('.NET'), ('SQL'), ('Java'), ('Git'),
    ('Project Management'), ('Communication'), ('Leadership'), ('Problem Solving'),
    ('Microsoft Excel'), ('Power BI'), ('Data Analysis'), ('Machine Learning'),
    ('AWS'), ('Docker');
GO

/*
==========================================
Interview Module — Reference Queries
(FOR REFERENCE ONLY — do not execute as-is.
 These use @param variables that must be declared first.)
==========================================

-- 1. Get all interviews
SELECT * FROM Interview;

-- 2. Get interview by ID
SELECT * FROM Interview WHERE InterviewId = @InterviewId;

-- 3. Get upcoming interviews (future scheduled)
SELECT * FROM Interview
WHERE ScheduledDate >= GETDATE()
ORDER BY ScheduledDate ASC;

-- 4. Get today's interviews
SELECT * FROM Interview
WHERE CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE);

-- 5. Schedule or reschedule interview
UPDATE Interview SET
    ScheduledDate = @ScheduledDate,
    Duration = @Duration,
    Location = @Location,
    MeetingLink = @MeetingLink,
    Status = @Status
WHERE InterviewId = @InterviewId;

-- 6. Cancel interview
UPDATE Interview SET Status = 'Cancelled' WHERE InterviewId = @InterviewId;

-- 7. Insert interview evaluation
INSERT INTO InterviewEvaluation
    (InterviewId, InterviewerId, TechnicalScore, CommunicationScore,
     ProblemSolvingScore, CultureFitScore, OverallScore, Recommendation, Comments, SubmittedAt)
VALUES
    (@InterviewId, @InterviewerId, @TechnicalScore, @CommunicationScore,
     @ProblemSolvingScore, @CultureFitScore, @OverallScore, @Recommendation, @Comments, GETDATE());

-- 8. Get evaluation by interview ID
SELECT * FROM InterviewEvaluation WHERE InterviewId = @InterviewId;

-- 9. Update evaluation
UPDATE InterviewEvaluation SET
    InterviewerId = @InterviewerId,
    TechnicalScore = @TechnicalScore,
    CommunicationScore = @CommunicationScore,
    ProblemSolvingScore = @ProblemSolvingScore,
    CultureFitScore = @CultureFitScore,
    OverallScore = @OverallScore,
    Recommendation = @Recommendation,
    Comments = @Comments,
    SubmittedAt = GETDATE()
WHERE InterviewId = @InterviewId;

-- 10. Hiring manager dashboard summary counts
SELECT
    COUNT(*) AS TotalInterviews,
    SUM(CASE WHEN CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodaysInterviews,
    SUM(CASE WHEN ScheduledDate >= GETDATE() THEN 1 ELSE 0 END) AS UpcomingInterviews,
    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedInterviews,
    SUM(CASE WHEN Status = 'Pending Feedback' THEN 1 ELSE 0 END) AS PendingFeedback
FROM Interview;

*/
