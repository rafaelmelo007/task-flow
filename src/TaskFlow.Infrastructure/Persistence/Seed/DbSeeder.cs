using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Persistence.Seed;

public class DbSeeder
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public DbSeeder(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(ct)) return;

        var demoUser = new User { Email = "demo@taskflow.app", PasswordHash = _hasher.Hash("Demo123!") };
        _db.Users.Add(demoUser);

        var tasks = new[]
        {
            TaskItem.Create("Design new dashboard layout", "Create wireframes for the new analytics dashboard", TaskItemStatus.InProgress, TaskPriority.High, DateTime.UtcNow.AddDays(2), demoUser.Id),
            TaskItem.Create("Write unit tests for API", "Achieve 80% code coverage on the task endpoints", TaskItemStatus.Todo, TaskPriority.High, DateTime.UtcNow.AddDays(5), demoUser.Id),
            TaskItem.Create("Update dependencies", "Bump all NuGet packages to latest stable", TaskItemStatus.Done, TaskPriority.Low, null, demoUser.Id),
            TaskItem.Create("Fix login page styling", "Align the form to center on mobile viewports", TaskItemStatus.Todo, TaskPriority.Medium, DateTime.UtcNow.AddDays(-1), demoUser.Id),
            TaskItem.Create("Deploy to staging", "Push the latest build to the staging environment", TaskItemStatus.InProgress, TaskPriority.High, DateTime.UtcNow.AddDays(1), demoUser.Id),
            TaskItem.Create("Document REST API", "Add OpenAPI descriptions to all endpoints", TaskItemStatus.Todo, TaskPriority.Low, null, demoUser.Id),
        };

        _db.Tasks.AddRange(tasks);
        await _db.SaveChangesAsync(ct);
    }
}
