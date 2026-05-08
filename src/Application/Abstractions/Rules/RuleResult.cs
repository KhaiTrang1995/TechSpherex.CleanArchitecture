namespace TechSpherex.CleanArchitecture.Application.Abstractions.Rules;

/// <summary>
/// Result of a rule engine evaluation.
/// Contains the outcome and any violations found.
/// </summary>
public sealed class RuleResult
{
    public bool IsValid => Violations.Count == 0;
    public IReadOnlyList<RuleViolation> Violations { get; }

    private RuleResult(IReadOnlyList<RuleViolation> violations) => Violations = violations;

    public static RuleResult Pass() => new([]);
    public static RuleResult Fail(IReadOnlyList<RuleViolation> violations) => new(violations);
    public static RuleResult Fail(string ruleCode, string message) =>
        new([new RuleViolation(ruleCode, message)]);
}

/// <summary>A single rule violation with code and message.</summary>
public sealed record RuleViolation(string RuleCode, string Message);
