#pragma warning disable MA0048 // File name is ResultGeneric.cs but type is Result<T> — generic file name convention differs

namespace TaskFlow.Application.Common.Models;

#pragma warning disable CA1000 // Static members on generic types — factory pattern is intentional here

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string error, ResultErrorType errorType) { IsSuccess = false; Error = error; ErrorType = errorType; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.General) => new(error, errorType);
    public static Result<T> NotFound(string error = "Not found.") => new(error, ResultErrorType.NotFound);
    public static Result<T> Conflict(string error) => new(error, ResultErrorType.Conflict);
    public static Result<T> Unauthorized(string error = "Unauthorized.") => new(error, ResultErrorType.Unauthorized);
    public static Result<T> ValidationError(string error) => new(error, ResultErrorType.Validation);
}

#pragma warning restore CA1000
#pragma warning restore MA0048
