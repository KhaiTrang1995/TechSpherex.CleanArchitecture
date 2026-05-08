using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TechSpherex.CleanArchitecture.Application.Abstractions.Rules;

namespace TechSpherex.CleanArchitecture.Infrastructure.Rules;

/// <summary>
/// Configuration-driven Rule Engine implementation.
/// Rules are defined in appsettings.json under the "RuleEngine:RuleSets" section.
///
/// Supports operators: ==, !=, &gt;, &lt;, &gt;=, &lt;=, Contains, StartsWith, EndsWith, IsNull, IsNotNull.
/// Supports AND/OR logic between conditions via the "Operator" field on the rule set.
/// </summary>
public sealed partial class RuleEngine : IRuleEngine
{
    private readonly Dictionary<string, RuleSetConfig> _ruleSets;
    private readonly ILogger<RuleEngine> _logger;

    public RuleEngine(IConfiguration configuration, ILogger<RuleEngine> logger)
    {
        _logger = logger;
        _ruleSets = new Dictionary<string, RuleSetConfig>(StringComparer.OrdinalIgnoreCase);

        var section = configuration.GetSection("RuleEngine:RuleSets");
        if (!section.Exists()) return;

        foreach (var ruleSetSection in section.GetChildren())
        {
            var ruleSet = new RuleSetConfig
            {
                Name = ruleSetSection.Key,
                Operator = Enum.TryParse<LogicOperator>(ruleSetSection["Operator"], true, out var op) ? op : LogicOperator.And,
                Rules = []
            };

            var rulesSection = ruleSetSection.GetSection("Rules");
            foreach (var ruleSection in rulesSection.GetChildren())
            {
                ruleSet.Rules.Add(new RuleConfig
                {
                    Code = ruleSection["Code"] ?? $"{ruleSet.Name}.Rule{ruleSet.Rules.Count}",
                    Field = ruleSection["Field"] ?? string.Empty,
                    ComparisonOperator = ruleSection["Operator"] ?? "==",
                    Value = ruleSection["Value"],
                    Message = ruleSection["Message"] ?? "Rule violated.",
                    Priority = int.TryParse(ruleSection["Priority"], out var p) ? p : 0
                });
            }

            ruleSet.Rules = [.. ruleSet.Rules.OrderBy(r => r.Priority)];
            _ruleSets[ruleSet.Name] = ruleSet;
        }

        _logger.LogInformation("RuleEngine loaded {Count} rule sets: {Names}",
            _ruleSets.Count, string.Join(", ", _ruleSets.Keys));
    }

    public RuleResult Evaluate(string ruleSetName, IDictionary<string, object?> context)
    {
        if (!_ruleSets.TryGetValue(ruleSetName, out var ruleSet))
        {
            _logger.LogWarning("Rule set '{RuleSetName}' not found", ruleSetName);
            return RuleResult.Pass();
        }

        var violations = new List<RuleViolation>();

        foreach (var rule in ruleSet.Rules)
        {
            var passed = EvaluateRule(rule, context);

            if (!passed)
            {
                violations.Add(new RuleViolation(rule.Code, rule.Message));

                // Short-circuit for AND logic: first failure = overall failure
                if (ruleSet.Operator == LogicOperator.And)
                    break;
            }
            else if (ruleSet.Operator == LogicOperator.Or)
            {
                // Short-circuit for OR logic: first pass = overall pass
                return RuleResult.Pass();
            }
        }

        // For OR logic: if we get here, all rules failed
        if (ruleSet.Operator == LogicOperator.Or && ruleSet.Rules.Count > 0)
            return RuleResult.Fail(violations);

        return violations.Count == 0 ? RuleResult.Pass() : RuleResult.Fail(violations);
    }

    public bool EvaluateExpression(string expression, IDictionary<string, object?> context)
    {
        // Parse simple "Field Operator Value" expressions
        var match = ExpressionRegex().Match(expression);
        if (!match.Success)
        {
            _logger.LogWarning("Invalid rule expression: '{Expression}'", expression);
            return false;
        }

        var rule = new RuleConfig
        {
            Code = "Inline",
            Field = match.Groups["field"].Value.Trim(),
            ComparisonOperator = match.Groups["op"].Value.Trim(),
            Value = match.Groups["value"].Value.Trim().Trim('"', '\''),
            Message = "Expression violated."
        };

        return EvaluateRule(rule, context);
    }

    public IReadOnlyList<string> GetRuleSetNames() => [.. _ruleSets.Keys];

    private static bool EvaluateRule(RuleConfig rule, IDictionary<string, object?> context)
    {
        context.TryGetValue(rule.Field, out var fieldValue);

        return rule.ComparisonOperator.ToUpperInvariant() switch
        {
            "ISNULL" => fieldValue is null,
            "ISNOTNULL" => fieldValue is not null,
            "==" or "EQ" => Equals(ConvertValue(fieldValue), ConvertValue(rule.Value)),
            "!=" or "NEQ" => !Equals(ConvertValue(fieldValue), ConvertValue(rule.Value)),
            ">" or "GT" => CompareNumeric(fieldValue, rule.Value) > 0,
            "<" or "LT" => CompareNumeric(fieldValue, rule.Value) < 0,
            ">=" or "GTE" => CompareNumeric(fieldValue, rule.Value) >= 0,
            "<=" or "LTE" => CompareNumeric(fieldValue, rule.Value) <= 0,
            "CONTAINS" => fieldValue?.ToString()?.Contains(rule.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            "STARTSWITH" => fieldValue?.ToString()?.StartsWith(rule.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            "ENDSWITH" => fieldValue?.ToString()?.EndsWith(rule.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            _ => false
        };
    }

    private static object? ConvertValue(object? value)
    {
        if (value is null) return null;
        var str = value.ToString();
        if (str is null) return null;

        if (decimal.TryParse(str, CultureInfo.InvariantCulture, out var d)) return d;
        if (bool.TryParse(str, out var b)) return b;
        return str;
    }

    private static int CompareNumeric(object? left, object? right)
    {
        var l = ToDecimal(left);
        var r = ToDecimal(right);
        if (l is null || r is null) return 0;
        return l.Value.CompareTo(r.Value);
    }

    private static decimal? ToDecimal(object? value)
    {
        if (value is null) return null;
        return decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    [GeneratedRegex(@"^(?<field>\w+)\s+(?<op>==|!=|>=|<=|>|<|EQ|NEQ|GT|LT|GTE|LTE|Contains|StartsWith|EndsWith|IsNull|IsNotNull)\s*(?<value>.*)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ExpressionRegex();
}

#region Configuration Models

internal enum LogicOperator { And, Or }

internal sealed class RuleSetConfig
{
    public string Name { get; set; } = default!;
    public LogicOperator Operator { get; set; } = LogicOperator.And;
    public List<RuleConfig> Rules { get; set; } = [];
}

internal sealed class RuleConfig
{
    public string Code { get; set; } = default!;
    public string Field { get; set; } = default!;
    public string ComparisonOperator { get; set; } = "==";
    public string? Value { get; set; }
    public string Message { get; set; } = "Rule violated.";
    public int Priority { get; set; }
}

#endregion
