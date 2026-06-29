using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; set; } = null!;

    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskItemStatus.Done;

    private TaskItem() { }

    public static TaskItem Create(
        string title,
        string? description,
        TaskItemStatus status,
        TaskPriority priority,
        DateTime? dueDate,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");

        return new TaskItem
        {
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            DueDate = dueDate,
            UserId = userId
        };
    }

    public void UpdateStatus(TaskItemStatus newStatus)
    {
        Status = newStatus;
        SetUpdatedAt();
    }

    public DomainResult Update(string title, string? description, TaskItemStatus status, TaskPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return DomainResult.Failure("Title cannot be empty.");

        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        SetUpdatedAt();
        return DomainResult.Success();
    }
}
