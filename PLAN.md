# TaskFlow — .NET Technical Interview Exercise Plan

> Implementation plan for the .NET Technical Interview Exercise: a simple full-stack task-management
> web app built with **.NET 9 / C#**, an **ASP.NET Core Web API**, and **SQLite**, following
> **Clean Architecture** (Domain → Application → Infrastructure → Api) and **TDD**, with an
> **Angular** front end. Engineering conventions (analyzers, layering discipline, dark-theme visual
> identity, no AutoMapper/MediatR) follow our team reference architecture.

---

## 0. Requirements Traceability Checklist

Every gradable item in the brief mapped to a concrete deliverable. Used as the final self-check before submission.

- [ ] **DB ≥2 tables** (`tasks`, `users`) — §7, `AppDbContext.cs`
- [ ] **Domain table PK + ≥2 fields** — §7 `TaskItem`
- [ ] **Web API CRUD, correct verbs/params/returns** — §7.1 contract, `TasksController.cs`
- [ ] **Second API: register, login, authorized + non-authorized endpoints** — §7.1, `AuthController.cs`
- [ ] **Data access layer** — `Infrastructure/Persistence/Repositories/*`
- [ ] **Business logic layer independent of data & API** — `Application` layer + `Architecture.Tests`
- [ ] **Unit tests for all components** (DAL, BLL, API) — §8 (genuine unit tests per component + integration)
- [ ] **Frontend framework integration** — Angular SPA (§4.4)
- [ ] **Responsive & user-friendly** — §6.3
- [ ] **Frontend CRUD for the use case** — §6.1
- [ ] **Structured code / clean state** — §6.4
- [ ] **README with setup** — §10.1 + §12.2 outline
- [ ] **Seeded demo data + credentials** — §11 (explicit creds, runs in demo)
- [ ] **GenAI: prompt + output + validation/correction/edge-case narrative** — §9 (`GENAI.md`)
- [ ] **Presentation: user story, design, architecture, demo** — §12 (`PRESENTATION.md`)
- [ ] **Clean Architecture** — §3 + automated rules
- [ ] **Sufficient tests / TDD** — §8 (targets per layer + commit history)
- [ ] **Code quality / best practices** — §2.4, analyzers
- [ ] **No console warnings (desired)** — §6.5 verification step
- [ ] **GenAI fluency & critical thinking** — §9

---

## 1. User Story (drives the whole project)

> **As** a busy professional,
> **I want** to register an account, log in securely, and manage my personal tasks
> (create, view, edit, complete, delete) with a title, description, status, priority and due date,
> **so that** I can keep track of my work in one place and never miss a deadline.

**Acceptance criteria**
- A visitor can register with email + password and is rejected if the email already exists (`409`).
- A registered user can log in and receives a JWT; an invalid password is rejected (`401`).
- An authenticated user can only see and modify **their own** tasks; accessing another user's task returns `404` (no existence leak — see §7.2).
- Tasks support full CRUD and can be filtered by status, searched by title, and paged (§7.1).
- Unauthenticated requests to protected endpoints return `401`.
- The app ships with seeded demo data and demo credentials (§11).

**Why a task manager?** It naturally satisfies every requirement (primary key + ≥2 fields, ownership/auth,
clean CRUD) and is trivial to demo live. It also aligns conceptually with the GenAI written exercise
(§9), so the same domain reasoning is reused.

---

## 2. Requirements (mapped to the exercise brief)

### 2.1 Functional — Backend
| # | Requirement (brief) | How it is satisfied |
|---|---------------------|---------------------|
| F1 | Data store with ≥2 tables: one domain, one users | SQLite DB with `Tasks` and `Users` tables |
| F2 | Domain table has PK + ≥2 other fields | `TaskItem`: `Id` (PK) + `Title`, `Description`, `Status`, `Priority`, `DueDate`, `UserId` |
| F3 | Web API with CRUD endpoints, correct HTTP verbs/params/returns | `TasksController` with explicit status-code contract (§7.1) |
| F4 | A **second** API for user creation, login, authorized + non-authorized endpoints | `AuthController`: `POST /register`, `POST /login` (anonymous) + `GET /me` (authorized); see §3.1 for the "second API" interpretation |
| F5 | Data access layer providing CRUD | `Infrastructure/Persistence/Repositories/*` behind interfaces |
| F6 | Business logic layer, independent of data layer & API | `Application` layer services + validators; no EF Core / ASP.NET references |
| F7 | Unit tests for all components | Genuine unit tests for Domain, Application, repositories, and controllers, plus an integration suite (§8) |

### 2.2 Functional — Frontend
| # | Requirement | How it is satisfied |
|---|------------|---------------------|
| F8 | Integrate a frontend framework | Angular SPA (brief allows "React, Vue, etc.") |
| F9 | Responsive & user-friendly | Tailwind responsive layout, mobile-first (§6) |
| F10 | CRUD for the use case | Task list + create/edit modal + delete confirm |
| F11 | Structured code, clean state | Standalone components + signal-based stores |

### 2.3 Functional — Submission
- README with setup instructions and architecture overview (outline in §12.2).
- Seeded demo data + explicit credentials, present when the app runs (§11).

### 2.4 Non-Functional
- **Clean Architecture** with enforced dependency direction (verified by architecture tests).
- **TDD** red-green-refactor for the Application + Domain layers; granular commit history preserved (not squashed) so the progression is verifiable (§8.1).
- Structured logging (Serilog), global exception handling via `IExceptionHandler` + `AddProblemDetails()` (RFC7807), correlation id.
- Static analysis: Roslynator, Meziantou, SonarAnalyzer, StyleCop, SecurityCodeScan via `Directory.Build.props`.
- No browser console warnings (graded bonus) — verified, not just asserted (§6.5); OnPush change detection.
- Dockerized as an optional convenience (the primary run path is local, §12).

### 2.5 Scope priority (must-have vs. nice-to-have)
A finished, warning-free core beats a half-built ambitious one (Functionality is graded), so work is prioritized and gold-plating is deliberately excluded:

| Priority | Items |
|----------|-------|
| **P0 — Must have** | Domain + Application layers (TDD), SQLite persistence, `TasksController` CRUD, `AuthController` register/login/me, JWT auth, ownership enforcement, unit + integration tests, Angular auth + task CRUD, seeded demo data, README |
| **P1 — Should have** | Filtering/search/paging, FluentValidation everywhere, RFC7807 errors, Serilog, Swagger w/ JWT, architecture tests, Playwright happy-path, GENAI.md, PRESENTATION.md |
| **P2 — Nice to have (optional)** | Docker/Nginx convenience setup, extra E2E flows |

**Explicitly out of scope** (would exceed "a simple web application"): refresh-token rotation, OpenTelemetry, coverage badges, multi-process deployment. A one-line "possible extension" note for refresh tokens lives in §7.3; nothing in the canonical design depends on it.

---

## 3. Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────┐
│                      TaskFlow.Api                         │  ASP.NET Core (MVC pipeline, API
│         Controllers · Middleware · DI composition        │  controllers, auth, Swagger, Serilog)
└───────────────┬─────────────────────────────────────────┘
                │ depends on
┌───────────────▼─────────────────────────────────────────┐
│                  TaskFlow.Infrastructure                  │  EF Core (SQLite), repositories,
│   AppDbContext · Repositories · JWT · BCrypt · Seeder     │  JWT generation, password hashing
└───────────────┬─────────────────────────────────────────┘
                │ implements interfaces from
┌───────────────▼─────────────────────────────────────────┐
│                   TaskFlow.Application                    │  Use-case services, DTOs,
│   Services · Validators · DTOs · Interfaces · Mapping     │  FluentValidation, abstractions
└───────────────┬─────────────────────────────────────────┘
                │ depends only on
┌───────────────▼─────────────────────────────────────────┐
│                     TaskFlow.Domain                       │  Entities, enums, domain rules,
│            Entities · Enums · Exceptions                  │  domain exceptions — zero deps
└─────────────────────────────────────────────────────────┘
```

**Rules (enforced by `TaskFlow.Architecture.Tests`):**
- `Domain` references nothing.
- `Application` references only `Domain`.
- `Infrastructure` references `Application` + `Domain` (and implements its interfaces).
- `Api` references everything but contains no business logic.
- Repository/JWT/clock **interfaces live in Application**; **implementations live in Infrastructure** (Dependency Inversion).

### 3.1 Key design decisions
- **"ASP.NET MVC, Web API":** In modern ASP.NET Core, MVC and Web API share one unified pipeline; `ApiController`-attributed controllers ARE the MVC pipeline (controllers, model binding, filters, action results) without server-rendered Razor views. Because the UI is the Angular SPA, no Razor views are needed; controllers return JSON. This is a conscious interpretation, stated so the panel reads it as a decision, not an omission.
- **"Second API":** the authentication API surface routes under `/api/auth/*` (its own controller, identity concern), distinct from the task resource API under `/api/tasks/*`, hosted in the same process (appropriate for an exercise). **Non-authorized:** `POST /api/auth/register`, `POST /api/auth/login`, `GET /health`. **Authorized:** `GET /api/auth/me` and all `/api/tasks/*`.
- **No AutoMapper / No MediatR:** explicit `.ToDto()` extension methods; plain injectable Application services (small dependency surface).
- **Error handling = Result pattern (single path):** expected outcomes (validation, not-found, conflict, cross-owner) flow through a `Result` object → controller → the right status code. **No `NotFoundException` for control flow.** `DomainException` is reserved for genuine invariant violations and, with any unexpected error, is mapped to `ProblemDetails` by the global `IExceptionHandler`.
- **Validation execution:** each Application service resolves and invokes its `IValidator<T>` at the top of the use-case and returns a validation `Result` on failure (validation thus lives in the business-logic layer, per the brief). Controllers translate that `Result` to `400 ValidationProblem`. (No reliance on the deprecated FluentValidation MVC auto-integration.)
- `record` types for all DTOs; `snake_case` table names; BCrypt password hashing; JWT bearer auth.

**Changed from the reference architecture (per request):** Vertical Slices → Clean Architecture layers; Minimal APIs → MVC/Web API controllers; PostgreSQL/Npgsql/Dapper → SQLite + EF Core only.

---

## 4. Technology Stack & Libraries

> **Version policy:** the whole stack is pinned to **current LTS** with **exact versions in
> `csproj`/`package.json` (no floating ranges or `latest`)** for reproducibility: **.NET 10 (LTS)** with
> ASP.NET framework packages on `10.0.x` (EF Core on `9.0.x`), **Node 20 LTS**, **Angular 20**. The SDK is pinned in `global.json`.

### 4.1 Backend (`net10.0`)
| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.x | EF Core SQLite provider (data store) |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.x | Migrations tooling |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.x | JWT bearer auth (brings `Microsoft.IdentityModel.*` transitively; use `JsonWebTokenHandler` — no separate `System.IdentityModel.Tokens.Jwt` reference) |
| `BCrypt.Net-Next` | 4.1.x | Password hashing |
| `FluentValidation` | 11.11.x | Declarative request validation |
| `FluentValidation.DependencyInjectionExtensions` | 11.11.x | Validator DI registration |
| `Serilog.AspNetCore` | 9.0.x | Structured logging + request logging |
| `Serilog.Sinks.File` | 6.0.x | Rolling file sink |
| `Swashbuckle.AspNetCore` | 7.x | Swagger / OpenAPI + JWT auth UI |
| `DotNetEnv` | 3.1.x | `.env` loading for local secrets |

### 4.2 Analyzers (root `Directory.Build.props`, exact pins)
`Roslynator.Analyzers`, `Meziantou.Analyzer`, `SonarAnalyzer.CSharp`, `StyleCop.Analyzers`, `SecurityCodeScan.VS2019` — each pinned to a specific version (no floating ranges).

### 4.3 Testing
| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.x | Test framework |
| `xunit.runner.visualstudio` | 3.x | Test runner |
| `Microsoft.NET.Test.Sdk` | 17.x | Test SDK |
| `FluentAssertions` | 7.1.x | Fluent assertions |
| `Moq` | 4.20.x | Mocking (service/repo unit tests) |
| `Bogus` | 35.6.x | Fake/seed data generation |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.x | Real SQLite for repository + integration tests |
| `Microsoft.AspNetCore.Mvc.Testing` | 9.0.x | `WebApplicationFactory` integration tests |
| `NetArchTest.Rules` | 1.3.x | Clean Architecture dependency rules |
| `coverlet.collector` | 6.x | Code coverage |

### 4.4 Frontend (Node 20 LTS, Angular 20)
| Package | Version | Purpose |
|---------|---------|---------|
| `@angular/*` | 20.x | SPA framework (standalone components) |
| `@ngrx/signals` | 20.x | Signal-based state stores (kept to mirror the reference architecture's store pattern; a native signal service would also suffice at this size) |
| `tailwindcss` + `@tailwindcss/postcss` + `postcss` + `autoprefixer` | pinned | Utility-first styling pipeline |
| `jwt-decode` | 4.x | Decode JWT on the client |
| Angular CLI unit-test builder (**Vitest**) | bundled | Unit tests (Angular's current default; replaces Karma/Jasmine) |
| `@playwright/test` | 1.5x | E2E tests |
| `eslint`, `angular-eslint`, `eslint-plugin-sonarjs`, `eslint-plugin-security` | pinned | Lint + quality + security |

---

## 5. Visual Identity (dark theme)

**Theme:** modern dark UI, calm and focused, purple accent.

### 5.1 Color tokens (`tailwind.config.js`)
| Token | Hex | Use |
|-------|-----|-----|
| `bg` | `#0e0f13` | App background |
| `surface` | `#16171d` | Cards / panels |
| `surface-2` | `#1e1f27` | Raised panels, inputs |
| `surface-3` | `#262731` | Hover / borders |
| `text` | `#e4e5eb` | Primary text |
| `text-2` | `#9395a5` | Secondary text |
| `text-3` | `#62647a` | Muted / placeholder |
| `accent` | `#6c5ce7` | Primary actions, focus |
| `accent-hover` | `#7c6ff7` | Hover state |
| `green` | `#00c48c` | Success / completed status |
| `amber` | `#f5a623` | Warning / in-progress |
| `red` | `#ff6b6b` | Errors / delete / overdue |
| `cyan` | `#22d3ee` | Info |
| `blue` | `#3b82f6` | Links / low priority |

### 5.2 Tokens
- **Radius:** `sm 6px`, `DEFAULT 8px`, `lg 12px`, `pill 20px`.
- **Shadows:** `card 0 1px 4px rgba(0,0,0,.25)`, `hover 0 2px 12px rgba(0,0,0,.35)`.
- **Typography:** system stack (`-apple-system, BlinkMacSystemFont, "Segoe UI", Inter, Roboto`); scale `badge .65rem` → `3xl 1.875rem`.
- **Motion:** `180ms cubic-bezier(.4,0,.2,1)` default transition.
- **Layout:** 240px sidebar, collapses to top bar below `900px`.

### 5.3 Status & priority color mapping (domain → UI)
| Task status | Color | Task priority | Color |
|-------------|-------|---------------|-------|
| `Todo` | `text-2` | `Low` | `blue` |
| `InProgress` | `amber` | `Medium` | `amber` |
| `Done` | `green` | `High` | `red` |
| Overdue (computed) | `red` | | |

---

## 6. UX Plan

### 6.1 Screens
1. **Login** — email/password, link to Register, inline validation, error toast on bad credentials. Demo credentials hint shown (§11).
2. **Register** — email/password/confirm, client + server validation, auto-login on success.
3. **Dashboard / Task List** — the core screen:
   - Top bar: app title, search box, "New Task" button, user menu (logout).
   - Filter pills: `All · Todo · In Progress · Done` (drive the `status` query param).
   - Task cards/rows: title, status badge, priority dot, due date (red if overdue), edit/delete actions.
   - Paging controls bound to `page`/`pageSize` (§7.1).
   - **Empty state** card when no tasks ("Create your first task").
   - **Loading spinner** during fetch; **disabled** state on mutations.
4. **Task Create/Edit Modal** — title, description, status, priority, due-date picker; reused for create and edit; client-side validation mirrors server rules.
5. **Delete Confirmation Dialog** — guards destructive action (no native `confirm()` dialogs).
6. **Toast Notifications** — success/error feedback for every mutation.

### 6.2 Key flows
- **Auth flow:** Register/Login → receive JWT access token in the response body → store it **in memory + mirror to `localStorage`** so a reload restores the session (documented XSS trade-off, mitigated by short token lifetime, §7.4, and strict encoding). `AuthGuard` admits to dashboard → `AuthInterceptor` attaches the bearer token → a `401` clears the token and redirects to login.
- **CRUD flow (request → reconcile):** the store calls the API, awaits the response, then updates state from the server result and shows a toast. Errors surface a toast and trigger a re-fetch to stay consistent. (Deliberately *not* optimistic-with-rollback — simpler and more robustly correct at demo scale, protecting the Functionality score.)
- **Ownership:** the UI never exposes another user's tasks; the backend enforces it regardless (§7.2).

### 6.3 Responsiveness & accessibility
- Mobile-first; sidebar → top bar < 900px; cards stack single-column on small screens.
- Keyboard-navigable modals (focus trap, ESC to close), `aria-label`s on icon buttons, visible focus ring using `accent`.
- Color is never the only signal (status has text label + color).

### 6.4 Component conventions
- Standalone components, `ChangeDetectionStrategy.OnPush`, `@if`/`@for` control flow (with `track`), `input()`/`computed()` signals, `inject()` for DI, Tailwind-only styling (no per-component SCSS).

### 6.5 "No console warnings" verification (graded bonus)
Verified, not asserted:
- A Playwright hook collects `page.on('console')` and **fails the test** on any `error`/`warning` during the happy path.
- Pre-empt common Angular noise: `track` in every `@for`, handle expected `401`s without logging, OnPush + signals to avoid `NG0xxx`, no unhandled promise rejections.
- Manual devtools pass documented as a checklist item in `PRESENTATION.md`.

---

## 7. Data Model & API Contract

**`Users`** (`users`)
| Column | Type | Notes |
|--------|------|-------|
| `Id` | TEXT (GUID) | PK |
| `Email` | TEXT | required; stored as a validated string; unique index with `COLLATE NOCASE` so duplicates are caught case-insensitively (`409`) |
| `PasswordHash` | TEXT | BCrypt |
| `CreatedAt` | TEXT (UTC) | audit |

**`Tasks`** (`tasks`)
| Column | Type | Notes |
|--------|------|-------|
| `Id` | TEXT (GUID) | PK |
| `Title` | TEXT | required, ≤200; `COLLATE NOCASE` to support case-insensitive search |
| `Description` | TEXT | optional, ≤2000 |
| `Status` | INTEGER | enum: Todo/InProgress/Done |
| `Priority` | INTEGER | enum: Low/Medium/High |
| `DueDate` | TEXT (UTC) | optional |
| `UserId` | TEXT | FK → Users.Id, indexed |
| `CreatedAt` / `UpdatedAt` | TEXT (UTC) | audit |

### 7.1 API contract (verbs, params, returns, status codes)
All responses use `application/json`; errors use RFC7807 `ProblemDetails`.

**Auth API — `/api/auth`**
| Method | Route | Auth | Request | Success | Errors |
|--------|-------|------|---------|---------|--------|
| POST | `/register` | Anonymous | `RegisterRequest` | `201 Created` + `AuthResponse` | `400` validation, `409` email exists |
| POST | `/login` | Anonymous | `LoginRequest` | `200 OK` + `AuthResponse` | `400`, `401` invalid credentials |
| GET | `/me` | Bearer | — | `200 OK` + `UserDto` | `401` |

**Tasks API — `/api/tasks`**
| Method | Route | Auth | Request | Success | Errors |
|--------|-------|------|---------|---------|--------|
| GET | `/?status=&search=&page=&pageSize=&sort=` | Bearer | query params | `200 OK` + `PagedResult<TaskDto>` | `400` bad query, `401` |
| GET | `/{id}` | Bearer | — | `200 OK` + `TaskDto` | `401`, `404` (incl. cross-owner) |
| POST | `/` | Bearer | `CreateTaskRequest` | `201 Created` + `Location` header + `TaskDto` | `400`, `401` |
| PUT | `/{id}` | Bearer | `UpdateTaskRequest` | `200 OK` + `TaskDto` | `400`, `401`, `404` |
| DELETE | `/{id}` | Bearer | — | `204 No Content` | `401`, `404` |

**Health — `/health`**: `GET`, anonymous, `200 OK`.

List query defaults: `page=1`, `pageSize=20` (max 100), `sort=createdAt:desc`. `status` accepts a `TaskItemStatus` value; **`search` matches `Title` via `EF.Functions.Like("%term%")` against the `COLLATE NOCASE` column** (correct case-insensitive behavior on SQLite — plain `string.Contains` translates to case-sensitive `instr`). Validated by `TaskQueryValidator`.

### 7.2 Ownership & error semantics
- Cross-user access (a user requesting/mutating a task they do not own) returns **`404 Not Found`**, not `403`, to avoid resource-enumeration leakage. Asserted by a dedicated test.
- Duplicate registration → `409 Conflict`; validation failures → `400` with field-level `errors`; auth failures → `401`.

### 7.3 Possible extension (out of scope)
Refresh-token rotation (HttpOnly cookie + `/api/auth/refresh`) is a natural next step but is intentionally **not** part of this exercise; baseline access-token-only auth (§6.2, §7.4) is complete and self-consistent on its own.

### 7.4 Auth & security parameters (concrete)
- **Password policy** (enforced in `RegisterRequestValidator`): min 8 chars, at least one letter and one number.
- **Hashing:** BCrypt with a **work factor of 11**.
- **JWT secret:** sourced from config/`.env` (never committed), **≥ 32 bytes**; bound via `AddOptions<JwtSettings>().Bind(...).ValidateDataAnnotations().ValidateOnStart()` so a missing/short key fails at startup, not first login.
- **Access-token lifetime:** **60 minutes**.
- **`TokenValidationParameters`:** validate issuer, audience, lifetime, and signing key; `ClockSkew` reduced to `TimeSpan.FromSeconds(30)`.

---

## 8. Test Plan

### 8.1 Approach — TDD (red → green → refactor)
Domain rules and Application use-cases are written **test-first**: create the test, add a failing
assertion, write the minimal implementation, refactor. **Commit history is preserved granular (not
squashed)** so the red/green progression is verifiable.

### 8.2 What is tested per layer
| Project | Targets | Strategy |
|---------|---------|----------|
| `TaskFlow.Domain.Tests` | Entity invariants, status transitions, overdue computation | Pure unit tests, no mocks |
| `TaskFlow.Application.Tests` | `TaskService` & `AuthService` use-cases, validators, mapping, ownership rules, error/notification paths | xUnit + Moq (mock repos/JWT/clock) + FluentAssertions + Bogus |
| `TaskFlow.Infrastructure.Tests` | Repository CRUD/filter/paging, EF mappings, seeder | Real **SQLite** (see §8.3 connection rule) |
| `TaskFlow.Api.Tests` | **(a) Controller unit tests** with mocked `ITaskService`/`IAuthService` — assert status-code mapping (`201`+`Location`, `204`, `400`, `404`), `[Authorize]` presence; **(b) Integration tests** end-to-end over the real pipeline (register→login→CRUD, `401`, cross-owner `404`, `409`) | (a) Moq; (b) `WebApplicationFactory` + SQLite |
| `TaskFlow.Architecture.Tests` | Dependency direction rules of §3 | `NetArchTest.Rules` |

This gives **genuine unit tests for every component the brief names** — data access layer (repository tests), business logic (service tests), and API endpoints (controller unit tests) — with integration tests layered on top for fidelity.

### 8.3 SQLite test rule (correctness)
In-memory SQLite is destroyed when its connection closes, and EF Core opens/closes connections between
operations. `SqliteTestFixture` and `CustomWebApplicationFactory` therefore create a **single
`SqliteConnection("DataSource=:memory:")`, call `Open()` and keep it open for the fixture's lifetime**,
pass that open connection to `UseSqlite(...)`, and call `EnsureCreated()` once. Each test gets isolation
via a fresh fixture/transaction.

### 8.4 Coverage targets (all layers)
- **Domain + Application:** ≥ **85%** (core business logic).
- **Infrastructure + Api:** ≥ **70%**.
- **Solution overall floor:** ≥ **75%**, collected via coverlet and reported in CI.

### 8.5 Frontend tests
- **Unit (Vitest), named explicitly:** `auth.store` (login/logout/expiry), `auth.guard` (redirect when unauthenticated), `auth.interceptor` (attaches bearer; `401`→logout), `tasks.store` (filter/search/paging state, request→reconcile), `task-form.component` (validation). Modest, proportional coverage expectation (not backend-equal).
- **E2E (Playwright):** happy path — login with seeded creds → create → edit → mark done → delete — plus the console-warning assertion (§6.5).

### 8.6 Test conventions
- Naming: `MethodUnderTest_Scenario_ExpectedResult` (e.g. `CreateAsync_WhenTitleEmpty_ReturnsValidationError`).
- Arrange-Act-Assert; one logical assertion group per test.
- Traits: `[Trait("Category","Unit|Integration|Architecture")]`.
- Deterministic time via injected `IDateTime` (no `DateTime.UtcNow` in logic).

### 8.7 CI gate (described in README)
Restore → build (warnings as errors in CI) → `dotnet test` (all projects) + coverage → frontend `lint` + `build` + `vitest` → Playwright smoke.

---

## 9. GenAI Written Exercise (separate deliverable — `GENAI.md`)

A graded criterion ("fluency... and critical thinking"). The deliverable is produced by **actually running
the prompt, pasting the real output, then critiquing that output** — not by pre-scripting findings.

### 9.1 The prompt (mirrors the brief's exact spec)
The brief's GenAI task defines a task with `title`, `description`, `status`, `due_date`, associated with a
user ("assume basic User model exists"). The prompt used will request exactly that:
> "Generate a RESTful API in ASP.NET Core (.NET 9, C#) for a simple task-management system. A `Task`
> has `title`, `description`, `status`, and `due_date`, and is associated with a user (assume a basic
> `User` model already exists). Provide full CRUD with correct HTTP verbs and status codes
> (`201`+`Location` on create, `204` on delete, `404` when not found), constructor DI, async/await,
> request DTOs with validation, and `ProblemDetails` errors. Do not use AutoMapper or MediatR."

Rationale captured in the doc: the prompt is specific (framework, exact fields, status codes, explicit
"don'ts") because under-constrained prompts produce generic, insecure scaffolds. The doc also notes that
**ownership/authorization is deliberately added by me on top of the AI output** (the brief's GenAI task
doesn't mention it) — demonstrating critical thinking beyond the prompt.

### 9.2 Representative output
`GENAI.md` pastes the real representative sample the tool returns (a controller + service + validator).

### 9.3 Critical evaluation — how the output was validated & corrected
Written **against the captured output**. As a checklist of issues to look for (and to document if present):
- missing ownership filter on read/update (add `UserId` scoping, `404` on mismatch);
- validation as entity data-annotations instead of request-DTO validators in the business layer;
- in-memory filtering / over-fetch instead of pushing filters into the query;
- DTOs leaking the entity / related user fields (introduce explicit `Dto` + `.ToDto()`);
- `[Authorize]` missing on mutations / no `401` handling.

For each issue actually found, the doc shows the before/after and explains the reasoning.

### 9.4 Edge cases / auth / validation handling
Documents: empty/oversize fields, invalid enum values, **`due_date` in the past on create** (rejected;
**not** enforced on update so already-overdue tasks remain editable), duplicate email on register,
expired/missing JWT, and cross-user access — each tied to a validator or test.

---

## 10. Files To Be Created

> Brief description of every file. Generated artifacts (EF migrations, `node_modules`, `dist`) are noted but not individually listed.

### 10.1 Repository root
| File | Description |
|------|-------------|
| `TaskFlow.sln` | Solution file referencing all backend + test projects |
| `global.json` | Pins the .NET 9 SDK version |
| `Directory.Build.props` | Shared build settings + pinned analyzer packages |
| `.editorconfig` | Formatting and analyzer severity rules |
| `.gitignore` | Ignore `bin/`, `obj/`, `node_modules/`, `*.db`, `.env`, etc. |
| `.env.template` | JWT secret, connection string, CORS origins |
| `docker-compose.yml` | Optional one-command demo (API + web) |
| `README.md` | Setup, run, test instructions, architecture overview, demo creds (outline §12.2) |
| `GENAI.md` | GenAI prompt, real output, and critical evaluation (§9) |
| `PRESENTATION.md` | Presentation outline + live-demo runbook (§12) |
| `PLAN.md` | This document |

### 10.2 `src/TaskFlow.Domain`
| File | Description |
|------|-------------|
| `TaskFlow.Domain.csproj` | Domain project (zero external dependencies) |
| `Common/BaseEntity.cs` | Base class: `Id`, `CreatedAt`, `UpdatedAt` |
| `Entities/TaskItem.cs` | Task aggregate with invariants & status transitions |
| `Entities/User.cs` | User entity (email string, password hash) |
| `Enums/TaskItemStatus.cs` | `Todo / InProgress / Done` |
| `Enums/TaskPriority.cs` | `Low / Medium / High` |
| `Exceptions/DomainException.cs` | Base domain exception (invariant violations only) |

### 10.3 `src/TaskFlow.Application`
| File | Description |
|------|-------------|
| `TaskFlow.Application.csproj` | Application project (references Domain only) |
| `DependencyInjection.cs` | Registers services + validators into DI |
| `Common/Models/Result.cs` | Result type for expected failures (validation/not-found/conflict) |
| `Common/Models/PagedResult.cs` | Pagination wrapper for list endpoints |
| `Common/Interfaces/ITaskRepository.cs` | Task data-access abstraction |
| `Common/Interfaces/IUserRepository.cs` | User data-access abstraction |
| `Common/Interfaces/IJwtTokenGenerator.cs` | JWT generation abstraction |
| `Common/Interfaces/IPasswordHasher.cs` | Password hash/verify abstraction |
| `Common/Interfaces/IDateTime.cs` | Clock abstraction (testable time) |
| `Common/Interfaces/ICurrentUser.cs` | Current authenticated user id accessor |
| `Tasks/Dtos/TaskDto.cs` | Task response record |
| `Tasks/Dtos/CreateTaskRequest.cs` | Create payload record |
| `Tasks/Dtos/UpdateTaskRequest.cs` | Update payload record |
| `Tasks/Dtos/TaskQuery.cs` | List filter/search/paging query record |
| `Tasks/Validators/CreateTaskRequestValidator.cs` | Create rules (incl. due-date-not-in-past) |
| `Tasks/Validators/UpdateTaskRequestValidator.cs` | Update rules (no past-due-date restriction) |
| `Tasks/Validators/TaskQueryValidator.cs` | Validates list query params |
| `Tasks/Mapping/TaskMappingExtensions.cs` | `TaskItem` → DTO `.ToDto()` mapping |
| `Tasks/Services/ITaskService.cs` | Task use-case contract |
| `Tasks/Services/TaskService.cs` | Task business logic (CRUD + ownership + filtering + validation) |
| `Auth/Dtos/RegisterRequest.cs` | Registration payload |
| `Auth/Dtos/LoginRequest.cs` | Login payload |
| `Auth/Dtos/AuthResponse.cs` | Token + user response |
| `Auth/Dtos/UserDto.cs` | User response record |
| `Auth/Validators/RegisterRequestValidator.cs` | Registration validation (incl. password policy §7.4) |
| `Auth/Validators/LoginRequestValidator.cs` | Login validation |
| `Auth/Mapping/UserMappingExtensions.cs` | `User` → `UserDto` mapping |
| `Auth/Services/IAuthService.cs` | Auth use-case contract |
| `Auth/Services/AuthService.cs` | Register/login/me business logic |

### 10.4 `src/TaskFlow.Infrastructure`
| File | Description |
|------|-------------|
| `TaskFlow.Infrastructure.csproj` | Infrastructure project (references Application) |
| `DependencyInjection.cs` | Registers DbContext, repositories, JWT, hasher, seeder |
| `Persistence/AppDbContext.cs` | EF Core context (SQLite) with `DbSet`s |
| `Persistence/Configurations/TaskItemConfiguration.cs` | EF mapping for `tasks` (incl. `Title COLLATE NOCASE`) |
| `Persistence/Configurations/UserConfiguration.cs` | EF mapping for `users` (incl. `Email` unique `COLLATE NOCASE`) |
| `Persistence/Repositories/TaskRepository.cs` | `ITaskRepository` impl (filtered/paged `EF.Functions.Like` query) |
| `Persistence/Repositories/UserRepository.cs` | `IUserRepository` impl |
| `Persistence/Seed/DbSeeder.cs` | Idempotent seed of demo user + tasks (Bogus) — §11 |
| `Persistence/Migrations/` | EF Core generated migrations (auto) |
| `Identity/JwtTokenGenerator.cs` | `IJwtTokenGenerator` impl (`JsonWebTokenHandler`) |
| `Identity/PasswordHasher.cs` | BCrypt `IPasswordHasher` impl (work factor 11) |
| `Services/DateTimeService.cs` | `IDateTime` system clock impl |
| `Settings/JwtSettings.cs` | Strongly-typed JWT config, validated on start (§7.4) |

### 10.5 `src/TaskFlow.Api`
| File | Description |
|------|-------------|
| `TaskFlow.Api.csproj` | Web API host project |
| `Program.cs` | Composition root: DI, auth, CORS, Serilog, Swagger, `AddProblemDetails`, migrate + idempotent seed |
| `appsettings.json` | Base configuration (incl. `Seed:Enabled`) |
| `appsettings.Development.json` | Dev overrides |
| `Properties/launchSettings.json` | Local launch profiles (fixed dev port, §13) |
| `Controllers/AuthController.cs` | `POST /register`, `POST /login` (anon) + `GET /me` (authorized) |
| `Controllers/TasksController.cs` | CRUD endpoints (authorized, owner-scoped) per §7.1 |
| `Controllers/HealthController.cs` | Liveness endpoint (anonymous) |
| `Infrastructure/GlobalExceptionHandler.cs` | `IExceptionHandler` → RFC7807 `ProblemDetails` + correlation id |
| `Services/CurrentUserService.cs` | `ICurrentUser` from `HttpContext` claims |
| `Extensions/SwaggerExtensions.cs` | Swagger + JWT bearer UI setup |
| `Extensions/CorsExtensions.cs` | CORS policy (allowed origins from config) — §13 |
| `Dockerfile` | Multi-stage build for the API (non-root runtime) |

### 10.6 Test projects (`tests/`)
| File | Description |
|------|-------------|
| `TaskFlow.Domain.Tests/*` | Entity/invariant unit tests |
| `TaskFlow.Application.Tests/Tasks/TaskServiceTests.cs` | Task use-case tests (mocked deps) |
| `TaskFlow.Application.Tests/Auth/AuthServiceTests.cs` | Auth use-case tests |
| `TaskFlow.Application.Tests/Validators/*` | Validator tests (incl. `TaskQueryValidator`) |
| `TaskFlow.Application.Tests/Common/TestData.cs` | Bogus fakers / builders shared by tests |
| `TaskFlow.Infrastructure.Tests/Repositories/*` | Repository unit tests on SQLite (incl. filter/paging) |
| `TaskFlow.Infrastructure.Tests/SqliteTestFixture.cs` | Open-connection in-memory SQLite fixture (§8.3) |
| `TaskFlow.Api.Tests/Controllers/*` | Controller **unit** tests with mocked services (status mapping, auth, `Location`) |
| `TaskFlow.Api.Tests/CustomWebApplicationFactory.cs` | Integration test host (open-connection SQLite, §8.3) |
| `TaskFlow.Api.Tests/AuthEndpointsTests.cs` | Integration: register/login/me (`201`/`401`/`409`) |
| `TaskFlow.Api.Tests/TaskEndpointsTests.cs` | Integration: CRUD + auth + validation + **cross-owner `404`** |
| `TaskFlow.Architecture.Tests/LayerDependencyTests.cs` | NetArchTest Clean Architecture rules |

### 10.7 Frontend (`web/`)
| File | Description |
|------|-------------|
| `package.json` | Angular deps (pinned) + scripts |
| `angular.json` | Angular CLI/build config (Vitest test builder) |
| `tsconfig.json` / `tsconfig.app.json` | TypeScript config |
| `tailwind.config.js` | Theme tokens from §5 |
| `postcss.config.js` | Tailwind/PostCSS pipeline |
| `src/index.html` | App shell |
| `src/main.ts` | Bootstrap standalone app |
| `src/styles.css` | Tailwind directives + base styles |
| `src/environments/environment*.ts` | API base URL per environment (§13) |
| `src/app/app.config.ts` | App providers (router, http, interceptors) |
| `src/app/app.routes.ts` | Route table with lazy features + guard |
| `src/app/app.component.ts` | Root layout (sidebar/top bar) |
| `src/app/core/auth/auth.store.ts` | Signal-based auth state store |
| `src/app/core/auth/auth.service.ts` | Auth HTTP calls |
| `src/app/core/auth/auth.guard.ts` | Route guard for protected pages |
| `src/app/core/auth/auth.interceptor.ts` | Attaches JWT + handles 401 |
| `src/app/core/api/api.config.ts` | API base URL token |
| `src/app/features/auth/login.component.ts` | Login screen |
| `src/app/features/auth/register.component.ts` | Register screen |
| `src/app/features/tasks/tasks.store.ts` | Task list state store (filter/search/paging) |
| `src/app/features/tasks/tasks.service.ts` | Task CRUD HTTP calls |
| `src/app/features/tasks/task-list.component.ts` | Dashboard list + filters + search + paging |
| `src/app/features/tasks/task-form.component.ts` | Create/edit modal |
| `src/app/shared/components/` | Reusable UI: button, input, card, badge, status-pill, priority-dot, confirm-dialog, toast, empty-state, loading-spinner |
| `src/app/shared/models/task.model.ts` | Task DTO/types |
| `src/app/shared/models/user.model.ts` | User/auth DTO types |
| `*.spec.ts` (next to units) | Vitest specs: auth.store, auth.guard, auth.interceptor, tasks.store, task-form |
| `e2e/tasks.spec.ts` | Playwright happy-path E2E + console-warning assertion |
| `Dockerfile` | Multi-stage Angular build served via Nginx |
| `nginx.conf` | SPA routing + API proxy |

---

## 11. Seeded Demo Data & Credentials

`DbSeeder` is **idempotent** (inserts only when the DB has no users) and runs on startup whenever
`Seed:Enabled=true` — set `true` in `appsettings.Development.json` **and** in the Docker demo config, so
the demo always has data regardless of `ASPNETCORE_ENVIRONMENT`. It seeds:
- **Demo user:** `demo@taskflow.app` / **`Demo123!`** (BCrypt-hashed at seed time).
- **~6 demo tasks** for that user spanning every status/priority, including one overdue and one due-soon, generated with Bogus.

These exact credentials appear on the login screen hint and in the README so the panel can sign in immediately.

---

## 12. Presentation & Demo Plan (`PRESENTATION.md`)

### 12.1 Live-demo runbook (local is primary; Docker optional)
**Primary path (always available):** `dotnet run` (API) + `ng serve` (web). Steps: register a new user →
log in → create/edit/complete/delete tasks → filter + search → demonstrate ownership (`401` when logged
out; `404` for another user's task via Swagger) → show Swagger UI. **Optional:** `docker compose up`
runs the same demo in containers.

### 12.2 Presentation outline
1. **User story** (§1) and the problem it solves.
2. **Architecture & design choices** — Clean Architecture layers, dependency inversion, why no MediatR/AutoMapper, SQLite rationale, the "second API" interpretation (§3.1).
3. **Live demo** (runbook above).
4. **Testing story** — TDD commit history, per-component unit tests, architecture tests, coverage report.
5. **GenAI walkthrough** — the prompt, the real AI output, and the defects caught/corrected (§9).
6. **Q&A prep** — anticipated code-review questions and answers.

### 12.3 README outline
Prerequisites (.NET 9 SDK, Node 20) · **local run (API + web) as the primary path** · optional Docker run · how to run tests + coverage · **demo credentials** · architecture diagram · API endpoint table (§7.1) · troubleshooting.

---

## 13. Integration: CORS, Base URL & Networking
- **Dev port:** the API dev URL is fixed in `launchSettings.json` (e.g. `http://localhost:5080`) so the Angular `environment.ts` base URL and the API CORS origin line up out of the box (supports the "no console warnings" criterion).
- **API CORS** reads allowed origins from config (`Cors__AllowedOrigins`, default `http://localhost:4200`), applied in `CorsExtensions.cs`.
- **Angular base URL** lives in `src/environments/environment.ts` (dev → `http://localhost:5080/api`) and `environment.prod.ts` (→ `/api`, proxied).
- **Docker (optional):** `nginx.conf` serves the SPA and proxies `/api` to the API container on one network, so no CORS is needed in the containerized demo.

---

## 14. Suggested Build Order (TDD-friendly)
1. Solution + `Directory.Build.props` + analyzers + `global.json` + architecture test (red).
2. Domain entities/enums (test-first) → architecture test green.
3. Application interfaces, DTOs, validators, `TaskService`/`AuthService` (test-first with mocks).
4. Infrastructure: `AppDbContext`, configurations, repositories, JWT, hasher (SQLite tests, §8.3).
5. Api: controllers, `GlobalExceptionHandler`, Program.cs wiring, CORS, Swagger, idempotent seeding (controller unit tests + integration tests; assert §7.1 status codes + cross-owner `404`).
6. Frontend: auth flow → task CRUD → filter/search/paging → polish (responsive, toasts, empty states) → unit specs → Playwright E2E + console-warning gate.
7. Write `GENAI.md` (run the prompt, critique real output), `PRESENTATION.md`, `README.md`; optional Docker; final pass against the §0 traceability checklist.
