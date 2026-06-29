using Bogus;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tests.Common;

public static class TestData
{
    public static User MakeUser(string? email = null) =>
        new Faker<User>()
            .RuleFor(u => u.Id, _ => Guid.NewGuid())
            .RuleFor(u => u.Email, f => email ?? f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _ => "hashed")
            .RuleFor(u => u.CreatedAt, _ => DateTime.UtcNow)
            .Generate();

    public static TaskItem MakeTask(Guid? userId = null)
    {
        var f = new Faker();
        return TaskItem.Create(
            f.Lorem.Sentence(3),
            f.Lorem.Paragraph(),
            f.PickRandom<TaskItemStatus>(),
            f.PickRandom<TaskPriority>(),
            null,
            userId ?? Guid.NewGuid());
    }
}
