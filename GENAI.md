# GenAI Written Exercise

## The Prompt

> "Generate a RESTful API in ASP.NET Core (.NET 10, C#) for a simple task-management system. A `Task`
> has `title`, `description`, `status`, and `due_date`, and is associated with a user (assume a basic
> `User` model already exists). Provide full CRUD with correct HTTP verbs and status codes
> (`201`+`Location` on create, `204` on delete, `404` when not found), constructor DI, async/await,
> request DTOs with validation, and `ProblemDetails` errors. Do not use AutoMapper or MediatR."

**Why this prompt?** It is specific about framework, exact fields, status codes, and explicit "don'ts". Under-constrained prompts produce generic, insecure scaffolds. The prompt intentionally omits authorization/ownership — a gap I evaluate critically below.

---

## Representative AI Output

```csharp
// AI-generated TasksController (representative sample)
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> Get(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        var task = await _taskService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem();
        var task = await _taskService.UpdateAsync(id, request);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _taskService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
```

---

## Critical Evaluation

### Issues Found & Corrected

**1. Missing ownership filter (critical security issue)**

The AI output fetches ALL tasks (`GetAllAsync()`) with no `userId` scoping. Any authenticated user can read/modify/delete any other user's tasks.

*Before (AI):*
```csharp
var tasks = await _taskService.GetAllAsync();
```

*After (corrected):*
```csharp
var tasks = await _taskService.GetAllAsync(userId: _currentUser.UserId);
```

Authorization is a mandatory addition: cross-user access returns `404` (not `403`) to prevent existence leakage. This is asserted by a dedicated integration test (`CrossOwnerAccess_Returns404`).

**2. Validation on entity annotations instead of request DTOs**

The AI used `[Required]`, `[MaxLength]` attributes directly on domain entity properties. This couples validation to the persistence layer and runs in the wrong place (model binding, not business logic).

*Before (AI):*
```csharp
public class Task
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}
```

*After (corrected):* FluentValidation validators in the Application layer, resolved by the service, not the framework:
```csharp
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DueDate)
            .Must(d => d == null || d.Value > dateTime.UtcNow)
            .WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
```

**3. In-memory filtering instead of database-level filtering**

The AI's `GetAllAsync()` fetched all records then filtered in memory — catastrophic at scale.

*After (corrected):* EF Core query with `EF.Functions.Like` for case-insensitive search, `Where(t => t.Status == ...)` pushed to SQL, `.Skip()/.Take()` for paging.

**4. DTOs leaking entity properties**

The AI returned the `Task` entity directly (or a thin wrapper that included `UserId`, `PasswordHash` from a navigation property). Explicit `TaskDto` records with a `.ToDto()` extension isolate the API surface.

**5. Missing `[Authorize]` on all mutation endpoints**

The AI omitted `[Authorize]` on the controller, meaning any unauthenticated request would succeed. The corrected design places `[Authorize]` at controller level, with `[AllowAnonymous]` only on register/login.

---

## Edge Cases Addressed

| Scenario | Handling |
|----------|----------|
| Empty/whitespace title | `FluentValidation` returns `400` with field-level error |
| Title > 200 chars | Validator rejects with `400` |
| `due_date` in the past (on create) | Validator rejects; NOT enforced on update so overdue tasks remain editable |
| Duplicate email on register | `AuthService` checks and returns `409 Conflict` |
| Expired/missing JWT | `JwtBearer` middleware returns `401 Unauthorized` |
| Cross-user task access | Service checks `task.UserId != currentUserId` → returns `404` (no `403` to prevent enumeration) |
| Invalid enum values in request | JSON deserialization fails with `400` before reaching service |

---

## What I Added Beyond the AI Output

1. **JWT authentication** with BCrypt password hashing — the prompt did not ask for it but a task API is meaningless without auth
2. **Ownership scoping** on every read/write operation
3. **Result pattern** instead of nullable returns (cleaner, no exception-as-control-flow)
4. **Architecture tests** to enforce Clean Architecture dependency rules at compile time
5. **Deterministic clock** (`IDateTime`) enabling time-sensitive tests without real-time coupling
6. **Seeded demo data** with known credentials for immediate demo
