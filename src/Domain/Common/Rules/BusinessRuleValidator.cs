namespace TechSpherex.CleanArchitecture.Domain.Common.Rules;

/// <summary>
/// Static helper for domain-level rule checking.
/// Entities call <c>BusinessRuleValidator.CheckRule(rule)</c> inside their methods.
/// </summary>
public static class BusinessRuleValidator
{
    /// <summary>
    /// Evaluates a rule and throws <see cref="BusinessRuleException"/> if broken.
    /// </summary>
    public static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleException(rule);
        }
    }

    /// <summary>
    /// Evaluates multiple rules ordered by priority and throws on the first broken rule.
    /// </summary>
    public static void CheckRules(params IBusinessRule[] rules)
    {
        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            CheckRule(rule);
        }
    }
}
