namespace TechSpherex.CleanArchitecture.Application.Abstractions.Rules;

/// <summary>
/// Application-layer rule engine abstraction.
/// Evaluates configurable business rules against a context object.
/// Supports dynamic rule definitions loaded from configuration.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Evaluates all rules in the specified rule set against the given context.
    /// </summary>
    /// <param name="ruleSetName">Name of the rule set (e.g. "TodoCreation", "OrderApproval").</param>
    /// <param name="context">Dictionary of facts/context values to evaluate rules against.</param>
    RuleResult Evaluate(string ruleSetName, IDictionary<string, object?> context);

    /// <summary>
    /// Evaluates a single inline rule expression against the given context.
    /// </summary>
    /// <param name="expression">A rule expression (e.g. "Amount > 1000").</param>
    /// <param name="context">Dictionary of facts/context values.</param>
    bool EvaluateExpression(string expression, IDictionary<string, object?> context);

    /// <summary>
    /// Gets all available rule set names.
    /// </summary>
    IReadOnlyList<string> GetRuleSetNames();
}
