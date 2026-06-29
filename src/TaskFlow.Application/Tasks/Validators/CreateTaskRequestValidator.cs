using FluentValidation;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Tasks.Dtos;

namespace TaskFlow.Application.Tasks.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(IDateTime dateTime)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.DueDate)
            .Must(d => d == null || d.Value.ToUniversalTime() > dateTime.UtcNow)
            .WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);
    }
}
