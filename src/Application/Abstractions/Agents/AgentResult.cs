namespace TechSpherex.CleanArchitecture.Application.Abstractions.Agents;

/// <summary>
/// Result of a skill agent execution.
/// </summary>
public sealed record AgentResult
{
    public required AgentResultStatus Status { get; init; }
    public required string Message { get; init; }
    public object? Data { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = [];

    public static AgentResult Success(string message, object? data = null) =>
        new() { Status = AgentResultStatus.Success, Message = message, Data = data };

    public static AgentResult Failure(string message) =>
        new() { Status = AgentResultStatus.Failure, Message = message };

    public static AgentResult NeedsMoreInfo(string message) =>
        new() { Status = AgentResultStatus.NeedsMoreInfo, Message = message };

    public static AgentResult PartialSuccess(string message, object? data = null) =>
        new() { Status = AgentResultStatus.PartialSuccess, Message = message, Data = data };
}

public enum AgentResultStatus
{
    Success,
    Failure,
    NeedsMoreInfo,
    PartialSuccess
}
