-- =======================================================
-- Admin Module Database Tables (SkillNet)
-- =======================================================

-- 1. Organization Table
CREATE TABLE Organization (
    OrganizationId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationName NVARCHAR(255) NOT NULL UNIQUE,
    Industry NVARCHAR(100) NULL,
    Website NVARCHAR(255) NULL,
    Logo NVARCHAR(255) NULL,
    Address NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Department Table
CREATE TABLE Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationId INT NOT NULL,
    DepartmentName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(OrganizationId)
);



-- 4. AuditLog Table
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

-- 5. SystemConfiguration Table
CREATE TABLE SystemConfiguration (
    [Key] NVARCHAR(100) PRIMARY KEY,
    [Value] NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL
);

-- Insert Default System Configurations
INSERT INTO SystemConfiguration ([Key], [Value], Description)
VALUES 
    ('ResumeMaxSize', '5MB', 'Maximum allowed file size for candidate resume uploads'),
    ('InterviewReminderHours', '24', 'Send automated interview reminders X hours before'),
    ('AllowMultipleApplications', 'False', 'Allow candidates to apply for multiple roles simultaneously');