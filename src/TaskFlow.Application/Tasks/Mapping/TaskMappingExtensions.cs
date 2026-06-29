using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Tasks.Mapping;

public static class TaskMappingExtensions
{
    public static TaskDto ToDto(this TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new(task.Id, task.Title, task.Description, task.Status, task.Priority,
            task.DueDate, task.IsOverdue, task.UserId, task.CreatedAt, task.UpdatedAt);
    }
}
