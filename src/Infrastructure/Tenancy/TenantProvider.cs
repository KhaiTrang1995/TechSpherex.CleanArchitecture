
using TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Resolves the current tenant from HTTP request context.
/// Resolution order: X-Tenant-Id header → JWT claim → Default tenant.
/// </summary>
namespace TechSpherex.CleanArchitecture.Infrastructure.Tenancy;
public sealed class TenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private const string TenantHeader = "X-Tenant-Id";
    private const string TenantClaimType = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? TenantId => ResolveTenantId();

    public TenantInfo? CurrentTenant => TenantId is not null
        ? new TenantInfo { Id = TenantId, Name = TenantId }
        : TenantInfo.Default;

    private string? ResolveTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return null;

        // 1. Check X-Tenant-Id header
        if (context.Request.Headers.TryGetValue(TenantHeader, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        // 2. Check JWT claim
        var tenantClaim = context.User.FindFirst(TenantClaimType);
        if (tenantClaim is not null && !string.IsNullOrWhiteSpace(tenantClaim.Value))
        {
            return tenantClaim.Value;
        }

        // 3. Default
        return TenantInfo.Default.Id;
    }
}
