CREATE DATABASE SkillNetDB;
GO

USE SkillNetDB;
GO

-- 1. Roles Table
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

-- 2. Users Table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Insert the mandatory assignment roles
INSERT INTO Roles (RoleName) VALUES ('Candidate'), ('Recruiter'), ('HiringManager'), ('Admin');
