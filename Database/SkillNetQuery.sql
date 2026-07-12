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
