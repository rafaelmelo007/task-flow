using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Dtos;

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);
