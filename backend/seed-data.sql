-- ============================================================
-- TaskFlow Seed Data — Demo / Testing
-- Run AFTER database-setup.sql
-- ============================================================
USE TaskFlowDb;
GO

DECLARE @AdminId  UNIQUEIDENTIFIER = NEWID();
DECLARE @Mgr1Id   UNIQUEIDENTIFIER = NEWID();
DECLARE @Dev1Id   UNIQUEIDENTIFIER = NEWID();
DECLARE @Dev2Id   UNIQUEIDENTIFIER = NEWID();
DECLARE @Proj1Id  UNIQUEIDENTIFIER = NEWID();
DECLARE @Proj2Id  UNIQUEIDENTIFIER = NEWID();

-- ── Users ────────────────────────────────────────────────────
-- Passwords are all: "Password1" (BCrypt hashed — regenerate via API in prod)
INSERT INTO Users (Id, Name, Email, PasswordHash, Role) VALUES
(@AdminId, 'Alice Admin',   'alice@taskflow.dev',
 '$2a$11$example_hash_replace_via_register_endpoint', 'Admin'),
(@Mgr1Id,  'Bob Manager',   'bob@taskflow.dev',
 '$2a$11$example_hash_replace_via_register_endpoint', 'Manager'),
(@Dev1Id,  'Carol Dev',     'carol@taskflow.dev',
 '$2a$11$example_hash_replace_via_register_endpoint', 'Member'),
(@Dev2Id,  'David Dev',     'david@taskflow.dev',
 '$2a$11$example_hash_replace_via_register_endpoint', 'Member');

-- ── Projects ─────────────────────────────────────────────────
INSERT INTO Projects (Id, Title, Description, CreatedBy) VALUES
(@Proj1Id, 'Customer Portal v2',
 'Rebuild the customer-facing portal with Angular 17 and improved UX.',
 @AdminId),
(@Proj2Id, 'API Gateway Migration',
 'Migrate legacy REST endpoints to the new .NET 8 API gateway with JWT auth.',
 @Mgr1Id);

-- ── Members ──────────────────────────────────────────────────
INSERT INTO ProjectMembers (ProjectId, UserId) VALUES
(@Proj1Id, @AdminId), (@Proj1Id, @Mgr1Id), (@Proj1Id, @Dev1Id),
(@Proj2Id, @Mgr1Id),  (@Proj2Id, @Dev1Id), (@Proj2Id, @Dev2Id);

-- ── Tasks — Project 1 ────────────────────────────────────────
INSERT INTO Tasks (Title, Description, Priority, Status, DueDate, Position, ProjectId, AssigneeId) VALUES
('Design new login page',      'Figma mockup → Angular component', 'High',   'Done',       DATEADD(DAY,-5, GETUTCDATE()), 0, @Proj1Id, @Dev1Id),
('Setup Angular Material theme','Configure custom indigo-pink theme', 'Medium','Done',       DATEADD(DAY,-3, GETUTCDATE()), 1, @Proj1Id, @Dev1Id),
('JWT auth interceptor',       'Auto-attach + refresh logic',      'High',   'InProgress', DATEADD(DAY, 2, GETUTCDATE()),  0, @Proj1Id, @Dev1Id),
('Build Kanban board',         'CDK DnD with 3 columns',           'High',   'InProgress', DATEADD(DAY, 3, GETUTCDATE()),  1, @Proj1Id, @Dev2Id),
('Dashboard analytics charts', 'Chart.js doughnut + bar charts',   'Medium', 'Todo',       DATEADD(DAY, 7, GETUTCDATE()),  0, @Proj1Id, @Dev2Id),
('Project invite flow',        'Email-based invite API + UI',      'Low',    'Todo',       DATEADD(DAY,10, GETUTCDATE()),  1, @Proj1Id, @Mgr1Id),
('Write API documentation',    'Swagger annotations + README',     'Low',    'Todo',       DATEADD(DAY,14, GETUTCDATE()),  2, @Proj1Id, NULL);

-- ── Tasks — Project 2 ────────────────────────────────────────
INSERT INTO Tasks (Title, Description, Priority, Status, DueDate, Position, ProjectId, AssigneeId) VALUES
('Map legacy endpoints',       'Audit all v1 API routes',          'High',   'Done',       DATEADD(DAY,-7, GETUTCDATE()), 0, @Proj2Id, @Dev2Id),
('EF Core DbContext setup',    'Entities, migrations, seed',       'High',   'InProgress', DATEADD(DAY, 1, GETUTCDATE()),  0, @Proj2Id, @Dev1Id),
('Implement refresh tokens',   'Store in DB, 7 day expiry',        'Medium', 'Todo',       DATEADD(DAY, 5, GETUTCDATE()),  0, @Proj2Id, @Dev2Id),
('Load testing with k6',       '1000 concurrent users benchmark',  'Low',    'Todo',       DATEADD(DAY,20, GETUTCDATE()),  1, @Proj2Id, NULL);

PRINT 'Seed data inserted. Use the /api/auth/register endpoint to create real users with hashed passwords.';
