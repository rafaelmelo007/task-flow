# TaskFlow — .NET 10 + Angular 20 Task Management App

A full-stack task management application built with **Clean Architecture**, demonstrating:
- .NET 10 / ASP.NET Core Web API (MVC controllers)
- SQLite via EF Core (Code-First)
- Angular 20 SPA (standalone components, signals)
- JWT Bearer authentication
- TDD with xUnit + FluentAssertions + Moq

---

## Architecture

```
TaskFlow.Api          ← ASP.NET Core controllers, Serilog, Swagger, auth middleware
  ↓ depends on
TaskFlow.Infrastructure ← EF Core, repositories, JWT, BCrypt, seeder
  ↓ implements interfaces from
TaskFlow.Application  ← Services (TaskService, AuthService), DTOs, validators, abstractions (no EF/ASP.NET refs)
  ↓ depends only on
TaskFlow.Domain       ← Entities, enums, domain exceptions (zero external deps)
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) + npm

---

## Running locally (primary path)

### 1. Backend API

```bash
# From repo root
cd src/TaskFlow.Api
dotnet run
# API listens on http://localhost:5080
# Swagger UI: http://localhost:5080/swagger
```

On first run in Development mode, the database is created automatically and seeded with demo data.

### 2. Frontend

```bash
# From repo root
cd web
npm install
npx ng serve
# App: http://localhost:4200
```

### Demo credentials
| Email | Password |
|-------|----------|
| demo@taskflow.app | Demo123! |

---

## Running tests

```bash
# All backend tests (Domain, Application, Infrastructure, API, Architecture)
dotnet test

# Individual suites
dotnet test tests/TaskFlow.Domain.Tests
dotnet test tests/TaskFlow.Application.Tests
dotnet test tests/TaskFlow.Infrastructure.Tests
dotnet test tests/TaskFlow.Api.Tests
dotnet test tests/TaskFlow.Architecture.Tests
```

Test summary (37 total, all passing):
- **Domain.Tests** — 7 entity/invariant unit tests
- **Application.Tests** — 13 unit tests (Moq) — `TaskService`, `AuthService`, validators
- **Infrastructure.Tests** — 4 repository integration tests (real SQLite in-memory)
- **Api.Tests** — 10 integration tests (WebApplicationFactory + real pipeline)
- **Architecture.Tests** — 3 Clean Architecture dependency rules (NetArchTest)

---

## API Reference

### Auth (`/api/auth`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/register` | Anonymous | Register a new user → 201 + JWT |
| POST | `/login` | Anonymous | Login → 200 + JWT |
| GET | `/me` | Bearer | Get current user info |

### Tasks (`/api/tasks`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Bearer | List tasks (filter, search, paging) |
| GET | `/{id}` | Bearer | Get task by ID |
| POST | `/` | Bearer | Create task → 201 + Location |
| PUT | `/{id}` | Bearer | Update task |
| DELETE | `/{id}` | Bearer | Delete task → 204 |

### Health
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/health` | Anonymous | Liveness check |

---

## Project Structure

```
/
├── src/
│   ├── TaskFlow.Domain/          # Entities, enums, exceptions
│   ├── TaskFlow.Application/     # Services, DTOs, validators, interfaces
│   ├── TaskFlow.Infrastructure/  # EF Core, repositories, JWT, BCrypt
│   └── TaskFlow.Api/             # Controllers, middleware, composition root
├── tests/
│   ├── TaskFlow.Domain.Tests/
│   ├── TaskFlow.Application.Tests/
│   ├── TaskFlow.Infrastructure.Tests/
│   ├── TaskFlow.Api.Tests/
│   └── TaskFlow.Architecture.Tests/
├── web/                          # Angular 20 SPA
└── TaskFlow.sln
```

---

## Design Decisions

- **No AutoMapper / No MediatR**: explicit `.ToDto()` extension methods, plain injectable Application services
- **Result pattern**: expected failures (validation, not-found, conflict) flow via `Result<T>` — no exceptions for control flow
- **Ownership enforcement**: cross-user task access returns `404` (no existence leak), asserted by integration tests
- **SQLite EnsureCreated**: uses `EnsureCreated()` for simplicity; production would use EF migrations
- **JWT via options**: `JwtBearerOptions` configured through `IOptions<JwtSettings>` to support test overrides
- **Architecture tests**: NetArchTest verifies that Domain ← Application ← Infrastructure ← Api dependency direction is enforced at build time
