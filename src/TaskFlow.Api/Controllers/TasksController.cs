using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Application.Tasks.Services;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ICurrentUser _currentUser;

    public TasksController(ITaskService taskService, ICurrentUser currentUser)
    {
        _taskService = taskService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TaskItemStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "createdAt:desc",
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var query = new TaskQuery(status, search, page, pageSize, sort);
        var result = await _taskService.GetTasksAsync(query, userId.Value, ct);
        return result.ErrorType switch
        {
            ResultErrorType.Validation => ValidationProblem(
                new ValidationProblemDetails(new Dictionary<string, string[]>(StringComparer.Ordinal) { [""] = new[] { result.Error! } })),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var result = await _taskService.GetByIdAsync(id, userId.Value, ct);
        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var result = await _taskService.CreateAsync(request, userId.Value, ct);
        return result.ErrorType switch
        {
            ResultErrorType.Validation => ValidationProblem(
                new ValidationProblemDetails(new Dictionary<string, string[]>(StringComparer.Ordinal) { [""] = new[] { result.Error! } })),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
        };
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var result = await _taskService.UpdateAsync(id, request, userId.Value, ct);
        return result.ErrorType switch
        {
            ResultErrorType.Validation => ValidationProblem(
                new ValidationProblemDetails(new Dictionary<string, string[]>(StringComparer.Ordinal) { [""] = new[] { result.Error! } })),
            ResultErrorType.NotFound => NotFound(),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => Ok(result.Value)
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var result = await _taskService.DeleteAsync(id, userId.Value, ct);
        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(),
            _ when !result.IsSuccess => Problem(result.Error),
            _ => NoContent()
        };
    }
}
