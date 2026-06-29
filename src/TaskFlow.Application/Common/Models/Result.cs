namespace TaskFlow.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result() { IsSuccess = true; }
    private Result(string error, ResultErrorType errorType) { IsSuccess = false; Error = error; ErrorType = errorType; }

    public static Result Success() => new();
    public static Result Failure(string error, ResultErrorType errorType = ResultErrorType.General) => new(error, errorType);
    public static Result NotFound(string error = "Not found.") => new(error, ResultErrorType.NotFound);
}
