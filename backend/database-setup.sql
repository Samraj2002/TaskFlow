-- TaskFlow Database Setup Script
-- Run this manually in SQL Server Management Studio (SSMS) if you prefer
-- over running EF migrations via CLI.
-- 
-- Or just use: dotnet ef database update
-- (EF will auto-generate and run migrations)

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaskFlowDb')
BEGIN
    CREATE DATABASE TaskFlowDb;
END
GO

USE TaskFlowDb;
GO

-- Users
CREATE TABLE Users (
    Id                  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name                NVARCHAR(100)   NOT NULL,
    Email               NVARCHAR(256)   NOT NULL,
    PasswordHash        NVARCHAR(MAX)   NOT NULL,
    Role                NVARCHAR(20)    NOT NULL DEFAULT 'Member',
    RefreshToken        NVARCHAR(MAX)   NULL,
    RefreshTokenExpiry  DATETIME2       NULL,
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

-- Projects
CREATE TABLE Projects (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title       NVARCHAR(200)   NOT NULL,
    Description NVARCHAR(MAX)   NOT NULL DEFAULT '',
    CreatedBy   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Projects_Users FOREIGN KEY (CreatedBy)
        REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- ProjectMembers (composite PK)
CREATE TABLE ProjectMembers (
    ProjectId   UNIQUEIDENTIFIER NOT NULL,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    JoinedAt    DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ProjectMembers PRIMARY KEY (ProjectId, UserId),
    CONSTRAINT FK_PM_Projects FOREIGN KEY (ProjectId)
        REFERENCES Projects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PM_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- Tasks
CREATE TABLE Tasks (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title       NVARCHAR(300)    NOT NULL,
    Description NVARCHAR(MAX)    NOT NULL DEFAULT '',
    Priority    NVARCHAR(10)     NOT NULL DEFAULT 'Medium', -- Low | Medium | High
    Status      NVARCHAR(20)     NOT NULL DEFAULT 'Todo',   -- Todo | InProgress | Done
    DueDate     DATETIME2        NULL,
    Position    INT              NOT NULL DEFAULT 0,
    ProjectId   UNIQUEIDENTIFIER NOT NULL,
    AssigneeId  UNIQUEIDENTIFIER NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Tasks_Projects FOREIGN KEY (ProjectId)
        REFERENCES Projects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Tasks_Users FOREIGN KEY (AssigneeId)
        REFERENCES Users(Id) ON DELETE SET NULL
);
GO

-- Comments
CREATE TABLE Comments (
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Content   NVARCHAR(MAX)    NOT NULL,
    TaskId    UNIQUEIDENTIFIER NOT NULL,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Comments_Tasks FOREIGN KEY (TaskId)
        REFERENCES Tasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- Indexes for performance
CREATE INDEX IX_Tasks_ProjectId  ON Tasks(ProjectId);
CREATE INDEX IX_Tasks_AssigneeId ON Tasks(AssigneeId);
CREATE INDEX IX_Tasks_Status     ON Tasks(Status);
CREATE INDEX IX_Comments_TaskId  ON Comments(TaskId);
GO

PRINT 'TaskFlowDb created successfully.';
