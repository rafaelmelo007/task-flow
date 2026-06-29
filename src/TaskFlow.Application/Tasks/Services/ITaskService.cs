using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Application.Tasks.Services;

/// <summary>
/// Task use-cases (CRUD + ownership + validation). Plain injectable service —
/// mirrors <see cref="Auth.Services.IAuthService"/>; no mediator indirection.
/// </summary>
public interface ITaskService
{
    Task<Result<PagedResult<TaskDto>>> GetTasksAsync(TaskQuery query, Guid userId, CancellationToken cancellationToken);
    Task<Result<TaskDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<Result<TaskDto>> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<Result<TaskDto>> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
