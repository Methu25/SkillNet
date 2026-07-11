USE SkillNetDB;
GO

-- Rebuild tables to support junction roles, lockout, and password resets
DROP TABLE IF EXISTS RefreshTokens;
DROP TABLE IF EXISTS UserRole;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Roles;

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

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

CREATE TABLE UserRole (
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, RoleID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE
);

CREATE TABLE RefreshTokens (
    TokenID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Token VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

INSERT INTO Roles (RoleName) VALUES ('Candidate'), ('Recruiter'), ('HiringManager'), ('Admin');
