namespace TechSpherex.CleanArchitecture.Domain.Common.Rules;

/// <summary>
/// Represents a business rule that can be evaluated against an entity.
/// Rules are composable building blocks for the Rule Engine.
/// </summary>
public interface IBusinessRule
{
    /// <summary>Unique rule identifier (e.g. "Todo.TitleRequired").</summary>
    string RuleCode { get; }

    /// <summary>Human-readable error message when the rule is violated.</summary>
    string Message { get; }

    /// <summary>Evaluation priority – lower values execute first.</summary>
    int Priority => 0;

    /// <summary>Evaluates the rule against the current context.</summary>
    bool IsBroken();
}
