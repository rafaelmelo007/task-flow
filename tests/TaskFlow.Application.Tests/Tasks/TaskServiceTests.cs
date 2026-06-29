using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Application.Tasks.Services;
using TaskFlow.Application.Tasks.Validators;
using TaskFlow.Application.Tests.Common;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Application.Tests.Tasks;

[Trait("Category", "Unit")]
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly Mock<IDateTime> _dtMock = new();

    public TaskServiceTests()
    {
        _dtMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
    }

    private TaskService CreateService() => new(
        _repoMock.Object,
        new CreateTaskRequestValidator(_dtMock.Object),
        new UpdateTaskRequestValidator(),
        new TaskQueryValidator());

    [Fact]
    public async Task GetByIdAsync_WhenNotOwner_ReturnsNotFound()
    {
        var task = TestData.MakeTask(Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        var result = await CreateService().GetByIdAsync(task.Id, Guid.NewGuid(), default);

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwner_ReturnsTask()
    {
        var userId = Guid.NewGuid();
        var task = TestData.MakeTask(userId);
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        var result = await CreateService().GetByIdAsync(task.Id, userId, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ReturnsValidationError()
    {
        var request = new CreateTaskRequest("", null, TaskItemStatus.Todo, TaskPriority.Low, null);

        var result = await CreateService().CreateAsync(request, Guid.NewGuid(), default);

        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ReturnsValidationError()
    {
        var request = new CreateTaskRequest("Title", null, TaskItemStatus.Todo, TaskPriority.Low, DateTime.UtcNow.AddDays(-1));

        var result = await CreateService().CreateAsync(request, Guid.NewGuid(), default);

        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaskFlow.Domain.Entities.TaskItem>(), default))
            .ReturnsAsync((TaskFlow.Domain.Entities.TaskItem t, CancellationToken _) => t);

        var request = new CreateTaskRequest("Test Task", null, TaskItemStatus.Todo, TaskPriority.Medium, null);
        var result = await CreateService().CreateAsync(request, userId, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task DeleteAsync_WhenNotOwner_ReturnsNotFound()
    {
        var task = TestData.MakeTask(Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        var result = await CreateService().DeleteAsync(task.Id, Guid.NewGuid(), default);

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotOwner_ReturnsNotFound()
    {
        var task = TestData.MakeTask(Guid.NewGuid());
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, default)).ReturnsAsync(task);

        var request = new UpdateTaskRequest("New Title", null, TaskItemStatus.Done, TaskPriority.High, null);
        var result = await CreateService().UpdateAsync(task.Id, request, Guid.NewGuid(), default);

        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
