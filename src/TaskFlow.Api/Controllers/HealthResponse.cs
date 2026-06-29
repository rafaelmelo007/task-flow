namespace TaskFlow.Api.Controllers;

/// <summary>Response payload for the liveness endpoint.</summary>
public sealed record HealthResponse(string Status);
