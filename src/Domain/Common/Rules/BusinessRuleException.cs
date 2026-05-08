namespace TechSpherex.CleanArchitecture.Domain.Common.Rules;

/// <summary>
/// Thrown when a domain business rule is violated.
/// Can be caught by the global exception handler for consistent API responses.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public IBusinessRule BrokenRule { get; }
    public string RuleCode => BrokenRule.RuleCode;

    public BusinessRuleException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }
}
