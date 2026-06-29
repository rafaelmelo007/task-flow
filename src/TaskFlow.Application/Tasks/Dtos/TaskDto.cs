using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Dtos;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    bool IsOverdue,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
