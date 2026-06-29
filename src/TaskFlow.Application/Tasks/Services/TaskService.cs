using FluentValidation;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Tasks.Dtos;
using TaskFlow.Application.Tasks.Mapping;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<TaskQuery> _queryValidator;

    public TaskService(
        ITaskRepository repo,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<TaskQuery> queryValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
    }

    public async Task<Result<PagedResult<TaskDto>>> GetTasksAsync(TaskQuery query, Guid userId, CancellationToken cancellationToken)
    {
        var validation = await _queryValidator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
            return Result<PagedResult<TaskDto>>.ValidationError(Join(validation));

        var paged = await _repo.GetPagedAsync(query, userId, cancellationToken).ConfigureAwait(false);
        var result = new PagedResult<TaskDto>(
            paged.Items.Select(t => t.ToDto()).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
        return Result<PagedResult<TaskDto>>.Success(result);
    }

    public async Task<Result<TaskDto>> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var task = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (task is null || task.UserId != userId)
            return Result<TaskDto>.NotFound($"Task {id} not found.");
        return Result<TaskDto>.Success(task.ToDto());
    }

    public async Task<Result<TaskDto>> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
            return Result<TaskDto>.ValidationError(Join(validation));

        TaskItem task;
        try
        {
            task = TaskItem.Create(
                request.Title,
                request.Description,
                request.Status,
                request.Priority,
                request.DueDate,
                userId);
        }
        catch (DomainException ex)
        {
            return Result<TaskDto>.ValidationError(ex.Message);
        }

        var created = await _repo.AddAsync(task, cancellationToken).ConfigureAwait(false);
        return Result<TaskDto>.Success(created.ToDto());
    }

    public async Task<Result<TaskDto>> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
            return Result<TaskDto>.ValidationError(Join(validation));

        var task = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (task is null || task.UserId != userId)
            return Result<TaskDto>.NotFound($"Task {id} not found.");

        var updateResult = task.Update(request.Title, request.Description, request.Status, request.Priority, request.DueDate);
        if (!updateResult.IsSuccess)
            return Result<TaskDto>.ValidationError(updateResult.Error!);

        await _repo.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        return Result<TaskDto>.Success(task.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var task = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (task is null || task.UserId != userId)
            return Result.NotFound($"Task {id} not found.");

        await _repo.DeleteAsync(task, cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static string Join(FluentValidation.Results.ValidationResult validation) =>
        string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
}
