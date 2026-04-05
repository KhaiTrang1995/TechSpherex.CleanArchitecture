namespace TechSpherex.CleanArchitecture.Domain.Common;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// Entities implementing this interface will automatically be filtered by TenantId.
/// </summary>
public interface ITenantEntity
{
    string TenantId { get; set; }
}
