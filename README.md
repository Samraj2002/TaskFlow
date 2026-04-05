# TaskFlow — Smart Team Task Manager

A full-stack SaaS app built with **Angular 17 + .NET 8 + SQL Server** following clean architecture.
Mimics real products like Jira/Trello — perfect for product company interviews and Fiverr projects.

---

## Tech Stack

| Layer       | Technology                              |
|-------------|----------------------------------------|
| Frontend    | Angular 17, Angular Material, CDK DnD  |
| Backend     | .NET 8 Web API, Clean Architecture     |
| Database    | SQL Server 2025 + Entity Framework Core 8 |
| Auth        | JWT Access Token + Refresh Token       |
| Charts      | Chart.js via ng2-charts                |
| API Docs    | Swagger / OpenAPI                      |

---

## Project Structure

```
TaskFlow/
├── backend/
│   ├── TaskFlow.sln
│   ├── TaskFlow.Domain/          ← Entities, Enums (no dependencies)
│   ├── TaskFlow.Infrastructure/  ← EF Core DbContext, SQL Server
│   ├── TaskFlow.Application/     ← Services, DTOs, Interfaces, JWT
│   └── TaskFlow.API/             ← Controllers, Program.cs, Swagger
│
└── frontend/
    └── src/app/
        ├── core/
        │   ├── models/           ← TypeScript interfaces
        │   ├── services/         ← AuthService, ProjectService, TaskService
        │   ├── interceptors/     ← JWT auto-attach + refresh
        │   └── guards/           ← authGuard, guestGuard, roleGuard
        ├── features/
        │   ├── auth/             ← Login, Register
        │   ├── dashboard/        ← Stats, Charts (Chart.js)
        │   ├── projects/         ← Project list, detail, invite
        │   └── tasks/            ← Kanban board (CDK Drag & Drop)
        └── shared/
            └── components/shell/ ← App layout with sidenav
```

---

## Database Schema

```sql
Users          → Id, Name, Email, PasswordHash, Role, RefreshToken, CreatedAt
Projects       → Id, Title, Description, CreatedBy(FK→Users), CreatedAt
ProjectMembers → ProjectId(FK), UserId(FK)  ← composite PK
Tasks          → Id, Title, Description, Priority, Status, DueDate,
                 Position, ProjectId(FK), AssigneeId(FK→Users), CreatedAt
Comments       → Id, Content, TaskId(FK), UserId(FK), CreatedAt
```

---

## Quick Start

### 1 — Backend Setup

**Prerequisites:** .NET 8 SDK, SQL Server 2025

```bash
cd backend/TaskFlow.API

# Restore NuGet packages
dotnet restore ../TaskFlow.sln

# Update connection string in appsettings.json if needed
# Default: Server=localhost;Database=TaskFlowDb;Trusted_Connection=True;TrustServerCertificate=True;

# Add EF Core migration & apply
dotnet ef migrations add InitialCreate --project ../TaskFlow.Infrastructure --startup-project .
dotnet ef database update --project ../TaskFlow.Infrastructure --startup-project .

# Run
dotnet run
# API → http://localhost:5000
# Swagger → http://localhost:5000/swagger
```

### 2 — Frontend Setup

**Prerequisites:** Node 18+, Angular CLI 17

```bash
# Install Angular CLI globally if needed
npm install -g @angular/cli@17

cd frontend
npm install
ng serve
# App → http://localhost:4200
```

---

## API Endpoints

### Auth
| Method | Route                | Access  | Description        |
|--------|----------------------|---------|--------------------|
| POST   | /api/auth/register   | Public  | Register new user  |
| POST   | /api/auth/login      | Public  | Login → JWT tokens |
| POST   | /api/auth/refresh    | Public  | Refresh access token |
| POST   | /api/auth/logout     | Bearer  | Invalidate refresh token |

### Projects
| Method | Route                              | Description              |
|--------|------------------------------------|--------------------------|
| GET    | /api/projects                      | All accessible projects  |
| POST   | /api/projects                      | Create project           |
| GET    | /api/projects/{id}                 | Get project by id        |
| PUT    | /api/projects/{id}                 | Update project           |
| DELETE | /api/projects/{id}                 | Delete project           |
| POST   | /api/projects/{id}/members         | Invite member by email   |
| DELETE | /api/projects/{id}/members/{uid}   | Remove member            |

### Tasks
| Method | Route                                        | Description           |
|--------|----------------------------------------------|-----------------------|
| GET    | /api/projects/{pid}/tasks                    | Get tasks (+ filters) |
| POST   | /api/projects/{pid}/tasks                    | Create task           |
| PUT    | /api/projects/{pid}/tasks/{id}               | Update task           |
| PATCH  | /api/projects/{pid}/tasks/{id}/move          | Move on Kanban board  |
| DELETE | /api/projects/{pid}/tasks/{id}               | Delete task           |
| GET    | /api/projects/{pid}/tasks/{id}/comments      | Get comments          |
| POST   | /api/projects/{pid}/tasks/{id}/comments      | Add comment           |

### Dashboard
| Method | Route           | Description               |
|--------|-----------------|---------------------------|
| GET    | /api/dashboard  | Stats, charts, my tasks   |

---

## Key Architecture Decisions

### Clean Architecture (Backend)
```
Controllers (API) → Application Services → Domain Entities
                  ↓
             Infrastructure (EF Core / SQL)
```
- **Domain** — pure C# entities, no dependencies
- **Infrastructure** — only knows about EF Core / SQL
- **Application** — business logic, DTOs, service interfaces
- **API** — thin controllers, dependency injection wiring

### Generic Response Wrapper
Every API endpoint returns:
```json
{
  "success": true,
  "message": "Project created.",
  "data": { ... },
  "errors": []
}
```

### JWT + Refresh Token Flow
1. Login → receive `accessToken` (60 min) + `refreshToken` (7 days)
2. Angular interceptor attaches Bearer token to every request
3. On 401 → interceptor auto-calls `/api/auth/refresh` and retries
4. Logout → refresh token is wiped from DB

### Angular Signals (v17)
- `AuthService` uses `signal<CurrentUser | null>` — reactive, no BehaviorSubject
- Components use `signal()` + `computed()` for local reactive state

### Kanban Drag & Drop
- `@angular/cdk/drag-drop` with `cdkDropListGroup`
- `transferArrayItem` for cross-column drops
- `PATCH /tasks/{id}/move` persists new status + position to SQL

---

## Adding to Your GitHub (Important for Fiverr/Interviews)

```bash
git init
git add .
git commit -m "feat: initial TaskFlow SaaS scaffold"
git remote add origin https://github.com/YOUR_USERNAME/taskflow.git
git push -u origin main
```

**Suggested README badges to add:**
```
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Angular](https://img.shields.io/badge/Angular-17-red)
![SQL Server](https://img.shields.io/badge/SQL_Server-2025-blue)
```

---

## Fiverr Gig Tips

When selling this type of project on Fiverr:
- **Gig title:** ".NET 8 + Angular 17 SaaS task manager / project management app"
- **Tags:** dotnet, angular, saas, sql-server, jwt, kanban, clean-architecture
- **Packages:** Basic (auth + CRUD) → Standard (+ Kanban) → Premium (+ Dashboard + deploy)
- Screenshot the Swagger UI and Kanban board for your gig thumbnail

---

## Interview Talking Points

✅ **Clean Architecture** — "I separated concerns into Domain, Application, Infrastructure, API layers so that switching from SQL Server to PostgreSQL only touches the Infrastructure project."

✅ **JWT + Refresh** — "Access tokens expire in 60 min. Angular's interceptor transparently refreshes them so users never get logged out unexpectedly."

✅ **Generic API Response** — "Every endpoint returns ApiResponse<T> — frontend always knows the shape, error handling is consistent."

✅ **Kanban Drag & Drop** — "I used Angular CDK's DragDrop module with connected lists. On drop I call PATCH /tasks/{id}/move to persist the new status and position."

✅ **EF Core Relationships** — "DeleteBehavior.Cascade for Task→Comment so deleting a task cleans up comments. DeleteBehavior.Restrict for Project→Creator to prevent orphan data."

✅ **Angular 17 Signals** — "Auth state is a signal — components read currentUser() reactively without subscribing to observables."
