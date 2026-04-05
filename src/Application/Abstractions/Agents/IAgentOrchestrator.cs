namespace TechSpherex.CleanArchitecture.Application.Abstractions.Agents;

/// <summary>
/// Orchestrates skill agent selection and execution.
/// Routes user prompts to the appropriate skill agent.
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// Execute a prompt by selecting and invoking the appropriate skill agent.
    /// </summary>
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a specific skill by ID.
    /// </summary>
    Task<AgentResult> ExecuteSkillAsync(string skillId, AgentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all available skills.
    /// </summary>
    IReadOnlyList<SkillInfo> GetAvailableSkills();
}

/// <summary>
/// Summary info about an available skill.
/// </summary>
public sealed record SkillInfo(string Id, string Name, string Description, IReadOnlyList<string> ExamplePrompts);
