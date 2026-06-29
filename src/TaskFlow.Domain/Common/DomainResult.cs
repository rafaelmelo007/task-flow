namespace TaskFlow.Domain.Common;

public class DomainResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private DomainResult(bool success, string? error = null)
    {
        IsSuccess = success;
        Error = error;
    }

    public static DomainResult Success() => new(true);
    public static DomainResult Failure(string error) => new(false, error);
}
