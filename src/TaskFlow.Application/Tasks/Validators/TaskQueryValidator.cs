using FluentValidation;
using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Application.Tasks.Validators;

public class TaskQueryValidator : AbstractValidator<TaskQuery>
{
    public TaskQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
