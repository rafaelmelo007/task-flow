using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;
    public TaskRepository(AppDbContext db) => _db = db;

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<TaskItem>> GetPagedAsync(TaskQuery query, Guid userId, CancellationToken ct = default)
    {
        var q = _db.Tasks.Where(t => t.UserId == userId).AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(t => EF.Functions.Like(t.Title, $"%{query.Search}%"));

        q = query.Sort switch
        {
            "createdAt:asc" => q.OrderBy(t => t.CreatedAt),
            "dueDate:asc" => q.OrderBy(t => t.DueDate),
            "dueDate:desc" => q.OrderByDescending(t => t.DueDate),
            "priority:desc" => q.OrderByDescending(t => t.Priority),
            _ => q.OrderByDescending(t => t.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<TaskItem>(items, total, query.Page, query.PageSize);
    }

    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(ct);
    }
}
