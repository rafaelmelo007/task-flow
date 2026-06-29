# Presentation Outline & Live Demo Runbook

## 1. User Story

> **As** a busy professional, **I want** to register an account, log in securely, and manage my personal tasks (create, view, edit, complete, delete) with a title, description, status, priority, and due date, **so that** I can keep track of my work in one place and never miss a deadline.

---

## 2. Architecture & Design Choices

### Clean Architecture layers

```
TaskFlow.Api         ← HTTP, auth middleware, Serilog, Swagger
  ↓
TaskFlow.Infrastructure ← EF Core + SQLite, JWT, BCrypt, Seeder
  ↓ implements interfaces from
TaskFlow.Application  ← Services (TaskService, AuthService), DTOs, FluentValidation, abstractions
  ↓
TaskFlow.Domain       ← Entities, enums, domain exceptions — zero external deps
```

**Enforced by `TaskFlow.Architecture.Tests`** (NetArchTest) — dependency violations fail the build.

### Key decisions

| Decision | Rationale |
|----------|-----------|
| No AutoMapper | Explicit `.ToDto()` extensions — less magic, easier to trace in code review |
| No MediatR | Plain injectable Application services (`TaskService`, `AuthService`) — one obvious call path per use-case, a smaller dependency graph, and no handler/pipeline indirection for a reviewer to chase. Each service resolves its `IValidator<T>` and enforces ownership directly. |
| Result pattern (not exceptions) | Expected outcomes (not-found, conflict, validation) flow as data, not exceptions |
| Ownership returns `404` not `403` | Prevents resource-enumeration by an attacker probing other users' IDs |
| `EnsureCreated()` vs. Migrate | Sufficient for demo; production would use `dotnet ef migrations add` |
| JWT bearer + BCrypt (work factor 11) | Standard, well-understood auth primitives |
| `IOptions<JwtSettings>` for JWT validation | Allows test overrides without hardcoding secrets in code |

---

## 3. Live Demo Runbook

### Prerequisites

- .NET 10 SDK installed
- Node 20+ + npm installed

### Step 1 — Start the API

```bash
cd src/TaskFlow.Api
dotnet run
# → http://localhost:5080
# → Swagger UI: http://localhost:5080/swagger
```

### Step 2 — Start the Angular app

```bash
cd web
npm install   # first time only
npx ng serve
# → http://localhost:4200
```

### Step 3 — Demo flow

1. Open **http://localhost:4200** — redirects to `/login`
2. Use demo credentials: `demo@taskflow.app` / `Demo123!`
3. View the pre-seeded tasks (multiple statuses + one overdue)
4. Filter by status (Todo / In Progress / Done)
5. Search by keyword in the top search bar
6. Create a new task (click **+ New Task** → fill form → submit → 201 Created)
7. Edit a task (✏️ button → change status to Done → save)
8. Delete a task (🗑️ button → confirm dialog → 204 No Content)
9. Log out (sidebar → Sign out) — redirected to `/login`, 401 on protected requests
10. In Swagger UI: demonstrate cross-owner 404
    - Register User B via `POST /api/auth/register`
    - Login as User B, copy token
    - Try `GET /api/tasks/{id_from_demo_user}` → **404 Not Found**

---

## 4. Testing Story

### TDD approach

Domain rules and Application use-cases were written test-first (red → green → refactor):
1. Write failing test asserting the expected behavior
2. Implement the minimal code to make it pass
3. Refactor

### Per-layer coverage

| Layer | Tests | Strategy |
|-------|-------|----------|
| Domain | 7 | Pure unit tests, no mocks — entity invariants, `IsOverdue`, `Update` validation |
| Application | 13 | xUnit + Moq — `TaskService`, `AuthService`, ownership rules, validators |
| Infrastructure | 4 | Real SQLite in-memory — repository CRUD, filter, search, paging |
| Api | 10 | `WebApplicationFactory` — full HTTP pipeline: register→login→CRUD, auth, cross-owner |
| Architecture | 3 | NetArchTest — Clean Architecture dependency rules enforced at build time |

**Total: 37 backend tests, all green on `dotnet test`.**

### Key test: Cross-owner `404`

```csharp
[Fact]
public async Task CrossOwnerAccess_Returns404()
{
    var (clientA, _) = await AuthorizedClientAsync();  // User A
    var (clientB, _) = await AuthorizedClientAsync();  // User B

    var createResp = await clientA.PostAsJsonAsync("/api/tasks", ...);
    var task = await createResp.Content.ReadFromJsonAsync<TaskDto>();

    var getResp = await clientB.GetAsync($"/api/tasks/{task!.Id}");
    getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);  // ownership enforced
}
```

---

## 5. GenAI Walkthrough

See `GENAI.md` for the full prompt, real AI output, and critical evaluation.

**Summary:** The AI produced working CRUD scaffolding but missed:
- Authentication/authorization entirely
- Per-user ownership filtering
- Business-layer validation (used entity annotations instead)
- Proper DTO separation

Each issue was identified, explained, and corrected with before/after code.

---

## 6. Q&A Preparation

**Q: Why SQLite instead of Postgres?**
A: Exercise scope — SQLite ships with EF Core provider, zero infrastructure. Production would swap the connection string + EF provider; the repository abstraction makes this transparent to the application layer.

**Q: Why not refresh tokens?**
A: Intentionally out of scope (noted in PLAN.md §2.5). Adding an `HttpOnly` cookie + `/api/auth/refresh` endpoint is the natural next step but wasn't required by the exercise spec.

**Q: How are architecture rules enforced?**
A: `TaskFlow.Architecture.Tests` uses NetArchTest to assert that `Domain` has no outbound dependencies, `Application` doesn't reference `Infrastructure` or `Api`, etc. These tests fail the CI gate if a developer accidentally adds a forbidden reference.

**Q: What would you change for production?**
A: EF migrations (instead of `EnsureCreated`), refresh-token rotation, HTTPS enforcement, rate limiting, structured log shipping (Serilog sinks to Seq/Elastic), container health checks, and environment-specific secret management via Azure Key Vault or AWS SSM.
