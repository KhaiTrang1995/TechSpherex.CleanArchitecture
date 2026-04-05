
using TechSpherex.CleanArchitecture.Application.Abstractions.Agents;
using Microsoft.Extensions.Logging;
namespace TechSpherex.CleanArchitecture.Application.Features.Agents;

/// <summary>
/// Default agent orchestrator that routes prompts to the appropriate skill agent.
/// Uses keyword matching for skill selection. In production, replace with
/// LLM-based intent detection (e.g., OpenAI function calling, Semantic Kernel).
/// </summary>
public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IEnumerable<ISkillAgent> _skills;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(IEnumerable<ISkillAgent> skills, ILogger<AgentOrchestrator> logger)
    {
        _skills = skills;
        _logger = logger;
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Agent orchestrator received prompt: {Prompt}", context.Prompt);

        // Simple skill matching by keyword (replace with LLM in production)
        var skill = SelectSkill(context.Prompt);

        if (skill is null)
        {
            var available = GetAvailableSkills();
            return AgentResult.NeedsMoreInfo(
                "I couldn't determine which skill to use. Available skills:\n" +
                string.Join("\n", available.Select(s => $"• **{s.Name}** — {s.Description}")));
        }

        _logger.LogInformation("Selected skill: {SkillId} ({SkillName})", skill.SkillId, skill.Name);

        try
        {
            var result = await skill.ExecuteAsync(context, cancellationToken);
            _logger.LogInformation("Skill {SkillId} completed with status: {Status}", skill.SkillId, result.Status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Skill {SkillId} failed with exception", skill.SkillId);
            return AgentResult.Failure($"An error occurred while executing '{skill.Name}': {ex.Message}");
        }
    }

    public async Task<AgentResult> ExecuteSkillAsync(string skillId, AgentContext context, CancellationToken cancellationToken)
    {
        var skill = _skills.FirstOrDefault(s => s.SkillId.Equals(skillId, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
            return AgentResult.Failure($"Skill '{skillId}' not found.");

        return await skill.ExecuteAsync(context, cancellationToken);
    }

    public IReadOnlyList<SkillInfo> GetAvailableSkills()
    {
        return _skills.Select(s => new SkillInfo(s.SkillId, s.Name, s.Description, s.ExamplePrompts)).ToList();
    }

    private ISkillAgent? SelectSkill(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        // Keyword-based routing (simple but effective for demos)
        foreach (var skill in _skills)
        {
            // Check if prompt matches any example prompt keywords
            var keywords = skill.Name.ToLowerInvariant().Split(' ')
                .Concat(skill.ExamplePrompts.SelectMany(p => p.ToLowerInvariant().Split(' ')))
                .Where(w => w.Length > 3)
                .Distinct();

            if (keywords.Any(kw => lower.Contains(kw)))
                return skill;
        }

        // Default to first skill if only one is registered
        if (_skills.Count() == 1)
            return _skills.First();

        return null;
    }
}
