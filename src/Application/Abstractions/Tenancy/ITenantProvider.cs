namespace TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;

/// <summary>
/// Provides the current tenant context for the request.
/// Resolved from HTTP headers, JWT claims, or subdomain.
/// </summary>
public interface ITenantProvider
{
    /// <summary>Current tenant ID. Null for system/global operations.</summary>
    string? TenantId { get; }

    /// <summary>Current tenant metadata.</summary>
    TenantInfo? CurrentTenant { get; }
}
