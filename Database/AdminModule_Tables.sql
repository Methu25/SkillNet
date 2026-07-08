-- =======================================================
-- Admin Module Database Tables (SkillNet)
-- =======================================================

-- 1. Organization Table
CREATE TABLE Organization (
    OrganizationId INT IDENTITY(1,1) PRIMARY KEY,
    OrganizationName NVARCHAR(255) NOT NULL,
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

-- 3. UserRole Table
CREATE TABLE UserRole (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 4. AuditLog Table
CREATE TABLE AuditLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    Timestamp DATETIME DEFAULT GETDATE()
);

-- 5. SystemConfiguration Table
CREATE TABLE SystemConfiguration (
    ConfigKey NVARCHAR(100) PRIMARY KEY,
    ConfigValue NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL,
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Insert Default System Configurations
INSERT INTO SystemConfiguration (ConfigKey, ConfigValue, Description)
VALUES 
    ('ResumeMaxSize', '5MB', 'Maximum allowed file size for candidate resume uploads'),
    ('InterviewReminder', '24', 'Send automated interview reminders X hours before'),
    ('MultipleApplications', 'True', 'Allow candidates to apply for multiple roles simultaneously');