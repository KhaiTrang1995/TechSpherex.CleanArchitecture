namespace TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;

/// <summary>
/// Represents tenant metadata.
/// </summary>
public sealed record TenantInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? ConnectionString { get; init; }
    public bool IsActive { get; init; } = true;

    /// <summary>Default tenant for single-tenant mode or fallback.</summary>
    public static TenantInfo Default => new()
    {
        Id = "default",
        Name = "Default Tenant",
        IsActive = true
    };
}
