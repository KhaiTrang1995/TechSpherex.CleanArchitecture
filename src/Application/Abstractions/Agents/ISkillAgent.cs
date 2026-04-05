namespace TechSpherex.CleanArchitecture.Application.Abstractions.Agents;

/// <summary>
/// Represents a skill that an AI agent can execute.
/// Each skill maps to a specific domain capability (e.g., manage todos, generate reports).
/// </summary>
public interface ISkillAgent
{
    /// <summary>Unique identifier for this skill.</summary>
    string SkillId { get; }

    /// <summary>Human-readable name.</summary>
    string Name { get; }

    /// <summary>Description of what this skill does.</summary>
    string Description { get; }

    /// <summary>Example prompts that trigger this skill.</summary>
    IReadOnlyList<string> ExamplePrompts { get; }

    /// <summary>
    /// Execute the skill with the given context.
    /// </summary>
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default);
}
