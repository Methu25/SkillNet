CREATE DATABASE SkillNetDB;
GO

USE SkillNetDB;
GO

-- 1. UserRole Table
CREATE TABLE UserRole (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    OrganizationId INT NULL,
    DepartmentId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES UserRole(RoleId),
    FOREIGN KEY (OrganizationId) REFERENCES Organization(OrganizationId),
    FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId)
);

-- Insert the mandatory assignment roles
INSERT INTO UserRole (RoleName, Description) VALUES 
('Candidate', 'Can apply for jobs and manage their profile'), 
('Recruiter', 'Can post jobs and review applications'), 
('HiringManager', 'Can conduct interviews and evaluations'), 
('Admin', 'Platform administration and governance');
