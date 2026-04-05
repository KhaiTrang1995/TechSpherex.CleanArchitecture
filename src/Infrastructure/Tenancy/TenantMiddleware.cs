
using TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware that validates and sets the tenant context for each request.
/// Must be registered before authentication middleware so tenant context is available early.
/// </summary>
namespace TechSpherex.CleanArchitecture.Infrastructure.Tenancy;
public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var tenantId = tenantProvider.TenantId;
        var tenant = tenantProvider.CurrentTenant;

        if (tenant is not null && !tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is inactive. Rejecting request.", tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "Forbidden",
                status = 403,
                detail = $"Tenant '{tenantId}' is not active."
            });
            return;
        }

        // Enrich Serilog log context with tenant info
        using (Serilog.Context.LogContext.PushProperty("TenantId", tenantId))
        {
            _logger.LogDebug("Request for tenant: {TenantId}", tenantId);
            await _next(context);
        }
    }
}
