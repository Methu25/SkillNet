USE SkillNetDB;
GO

-- Create Permissions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' and xtype='U')
BEGIN
    CREATE TABLE Permissions (
        PermissionID INT IDENTITY(1,1) PRIMARY KEY,
        PermissionName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(255) NULL
    );
END
GO

-- Create RolePermissions junction table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolePermissions' and xtype='U')
BEGIN
    CREATE TABLE RolePermissions (
        RoleID INT NOT NULL,
        PermissionID INT NOT NULL,
        PRIMARY KEY (RoleID, PermissionID),
        FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE,
        FOREIGN KEY (PermissionID) REFERENCES Permissions(PermissionID) ON DELETE CASCADE
    );
END
GO

-- Seed standard permissions
INSERT INTO Permissions (PermissionName, Description)
VALUES 
    ('ManageUsers', 'Create, update, delete, and manage users'),
    ('ManageRoles', 'Assign roles to users and manage permissions'),
    ('ManageOrganizations', 'Create and edit organizations and departments'),
    ('ManageSettings', 'Update global system configurations'),
    ('ViewAuditLogs', 'View system audit logs'),
    ('PostJobs', 'Create and publish job postings'),
    ('ReviewApplications', 'Review candidate applications'),
    ('ConductInterviews', 'Schedule and conduct interviews'),
    ('ApplyForJobs', 'Apply for open positions');
GO

-- Assign all Admin permissions to the Admin role (RoleID = 4)
-- Assuming RoleID 4 is Admin based on schema_v1_auth.sql
INSERT INTO RolePermissions (RoleID, PermissionID)
SELECT 4, PermissionID FROM Permissions WHERE PermissionName IN ('ManageUsers', 'ManageRoles', 'ManageOrganizations', 'ManageSettings', 'ViewAuditLogs');
GO
