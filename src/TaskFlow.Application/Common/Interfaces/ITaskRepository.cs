using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<TaskItem>> GetPagedAsync(TaskQuery query, Guid userId, CancellationToken ct = default);
    Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(TaskItem task, CancellationToken ct = default);
}
