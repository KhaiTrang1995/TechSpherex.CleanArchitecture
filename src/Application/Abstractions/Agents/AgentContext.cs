namespace TechSpherex.CleanArchitecture.Application.Abstractions.Agents;

/// <summary>
/// Context passed to a skill agent for execution.
/// </summary>
public sealed record AgentContext
{
    /// <summary>The user's natural language prompt.</summary>
    public required string Prompt { get; init; }

    /// <summary>Optional structured parameters extracted from the prompt.</summary>
    public Dictionary<string, object?> Parameters { get; init; } = [];

    /// <summary>The authenticated user ID (if available).</summary>
    public string? UserId { get; init; }

    /// <summary>The current tenant ID (if multi-tenant).</summary>
    public string? TenantId { get; init; }

    /// <summary>Conversation history for multi-turn interactions.</summary>
    public List<AgentMessage> ConversationHistory { get; init; } = [];
}

/// <summary>
/// A message in the agent conversation history.
/// </summary>
public sealed record AgentMessage(string Role, string Content, DateTimeOffset Timestamp);
