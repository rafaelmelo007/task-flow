using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Dtos;

public record TaskQuery(
    TaskItemStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20,
    string Sort = "createdAt:desc");
