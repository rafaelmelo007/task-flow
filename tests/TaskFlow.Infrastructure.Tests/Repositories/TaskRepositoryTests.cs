using FluentAssertions;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence.Repositories;
using Xunit;

namespace TaskFlow.Infrastructure.Tests.Repositories;

[Trait("Category", "Integration")]
public sealed class TaskRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly TaskRepository _repo;

    public TaskRepositoryTests()
    {
        _repo = new TaskRepository(_fixture.Context);
    }

    private User SeedUser()
    {
        var user = new User { Id = Guid.NewGuid(), Email = $"test{Guid.NewGuid()}@test.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        _fixture.Context.Users.Add(user);
        _fixture.Context.SaveChanges();
        return user;
    }

    [Fact]
    public async Task AddAsync_PersistsTask()
    {
        var user = SeedUser();
        var task = TaskItem.Create("Test", null, TaskItemStatus.Todo, TaskPriority.Medium, null, user.Id);

        await _repo.AddAsync(task);

        var found = await _repo.GetByIdAsync(task.Id);
        found.Should().NotBeNull();
        found!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task GetPagedAsync_FiltersbyStatus()
    {
        var user = SeedUser();
        await _repo.AddAsync(TaskItem.Create("A", null, TaskItemStatus.Todo, TaskPriority.Low, null, user.Id));
        await _repo.AddAsync(TaskItem.Create("B", null, TaskItemStatus.Done, TaskPriority.Low, null, user.Id));

        var result = await _repo.GetPagedAsync(new TaskQuery(Status: TaskItemStatus.Todo), user.Id);

        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("A");
    }

    [Fact]
    public async Task GetPagedAsync_SearchByTitle()
    {
        var user = SeedUser();
        await _repo.AddAsync(TaskItem.Create("Buy groceries", null, TaskItemStatus.Todo, TaskPriority.Low, null, user.Id));
        await _repo.AddAsync(TaskItem.Create("Write report", null, TaskItemStatus.Todo, TaskPriority.Low, null, user.Id));

        var result = await _repo.GetPagedAsync(new TaskQuery(Search: "groceries"), user.Id);

        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("Buy groceries");
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var user = SeedUser();
        var task = TaskItem.Create("Delete me", null, TaskItemStatus.Todo, TaskPriority.Low, null, user.Id);
        await _repo.AddAsync(task);

        await _repo.DeleteAsync(task);

        var found = await _repo.GetByIdAsync(task.Id);
        found.Should().BeNull();
    }

    public void Dispose() => _fixture.Dispose();
}
