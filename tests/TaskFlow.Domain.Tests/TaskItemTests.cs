using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.Domain.Tests;

[Trait("Category", "Unit")]
public class TaskItemTests
{
    [Fact]
    public void IsOverdue_WhenDueDateInPastAndNotDone_ReturnsTrue()
    {
        var task = TaskItem.Create("Test", null, TaskItemStatus.Todo, TaskPriority.Low, DateTime.UtcNow.AddDays(-1), Guid.NewGuid());
        task.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenDone_ReturnsFalse()
    {
        var task = TaskItem.Create("Test", null, TaskItemStatus.Done, TaskPriority.Low, DateTime.UtcNow.AddDays(-1), Guid.NewGuid());
        task.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_WhenNoDueDate_ReturnsFalse()
    {
        var task = TaskItem.Create("Test", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        task.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void Update_WithEmptyTitle_ReturnsDomainError()
    {
        var task = TaskItem.Create("Original", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        var result = task.Update("", null, TaskItemStatus.Todo, TaskPriority.Low, null);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Update_WithValidData_UpdatesProperties()
    {
        var task = TaskItem.Create("Old", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        var result = task.Update("New Title", "desc", TaskItemStatus.InProgress, TaskPriority.High, null);
        result.IsSuccess.Should().BeTrue();
        task.Title.Should().Be("New Title");
        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void UpdateStatus_ChangesStatus()
    {
        var task = TaskItem.Create("Test", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        task.UpdateStatus(TaskItemStatus.Done);
        task.Status.Should().Be(TaskItemStatus.Done);
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsDomainException()
    {
        Action act = () => TaskItem.Create("", null, TaskItemStatus.Todo, TaskPriority.Low, null, Guid.NewGuid());
        act.Should().Throw<TaskFlow.Domain.Exceptions.DomainException>();
    }
}
