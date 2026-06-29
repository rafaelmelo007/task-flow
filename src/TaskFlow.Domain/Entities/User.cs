using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<TaskItem> Tasks { get; } = [];
}
