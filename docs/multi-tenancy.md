# Multi-Tenancy Guide

## Overview

This template supports **shared-table multi-tenancy** — all tenants share the same database and tables, isolated by a `TenantId` column. EF Core global query filters ensure tenants can only see their own data.

## Architecture

```
Request → TenantMiddleware → Resolve Tenant
                                  │
                    ┌─────────────┼─────────────┐
                    ▼             ▼             ▼
              X-Tenant-Id    JWT Claim     Default
                Header      "tenant_id"    Tenant
                                  │
                                  ▼
                        ITenantProvider.TenantId
                                  │
                    ┌─────────────┼─────────────┐
                    ▼                           ▼
            SaveChangesAsync              Query Execution
          (auto-set TenantId)        (global query filter)
```

## Tenant Resolution

The `TenantProvider` resolves the current tenant in this order:

1. **HTTP Header**: `X-Tenant-Id: my-tenant`
2. **JWT Claim**: `tenant_id` claim in the bearer token
3. **Default**: Falls back to `"default"` tenant

### Using Headers

```bash
curl -X GET https://localhost:7200/api/todos \
  -H "Authorization: Bearer <token>" \
  -H "X-Tenant-Id: acme-corp"
```

### Using JWT Claims

Add `tenant_id` claim when generating tokens:

```csharp
// In TokenService, add to claims:
new Claim("tenant_id", user.TenantId)
```

## Making an Entity Tenant-Aware

### Step 1: Implement ITenantEntity

```csharp
public sealed class Product : AuditableEntity, ITenantEntity
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string TenantId { get; set; } = default!; // Required by ITenantEntity
}
```

### Step 2: That's It!

The global query filter and `SaveChangesAsync` override handle everything:

- **On Create**: `TenantId` is automatically set from `ITenantProvider`
- **On Query**: Only data matching the current `TenantId` is returned
- **On Update/Delete**: Global filter prevents cross-tenant access

## Configuration

In `appsettings.json`:

```json
{
  "MultiTenancy": {
    "Enabled": true,
    "DefaultTenantId": "default",
    "HeaderName": "X-Tenant-Id"
  }
}
```

## Cross-Tenant Queries (Admin)

For admin operations that need to query across tenants, you can bypass the global filter:

```csharp
// In a handler or service:
var allTodos = await _context.Todos
    .IgnoreQueryFilters()  // Bypass tenant filter
    .ToListAsync(ct);
```

> ⚠️ **Use `IgnoreQueryFilters()` only for admin operations.** Always verify the user has admin permissions first.

## Tenant Management

### Register a New Tenant

In production, you'd typically have a `Tenants` table and registration endpoint. Here's a simple example:

```csharp
// Domain entity
public sealed class Tenant : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Identifier { get; set; } = default!;  // Used as TenantId
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Per-Tenant Database (Advanced)

For stronger isolation, you can extend `ITenantProvider` to provide per-tenant connection strings:

```csharp
var tenant = tenantProvider.CurrentTenant;
if (tenant?.ConnectionString is not null)
{
    // Use tenant-specific database
    optionsBuilder.UseNpgsql(tenant.ConnectionString);
}
```

## Testing Multi-Tenancy

```csharp
[Fact]
public async Task Query_ShouldReturnOnlyCurrentTenantData()
{
    // Arrange: Seed data for two tenants
    await SeedTodo("Tenant A Todo", tenantId: "tenant-a");
    await SeedTodo("Tenant B Todo", tenantId: "tenant-b");
    
    // Act: Query as tenant-a
    SetCurrentTenant("tenant-a");
    var todos = await _context.Todos.ToListAsync();
    
    // Assert: Only tenant-a data returned
    todos.Should().AllSatisfy(t => t.TenantId.Should().Be("tenant-a"));
}
```

## Logging with Tenant Context

The `TenantMiddleware` automatically enriches Serilog log context with `TenantId`. All logs within a tenant request include the tenant identifier:

```json
{
  "Timestamp": "2026-04-05T12:00:00Z",
  "Level": "Information",
  "Message": "Created todo: 'Review PR'",
  "Properties": {
    "TenantId": "acme-corp",
    "UserId": "admin@acme-corp.com"
  }
}
```

This enables tenant-specific log filtering in Kibana or Grafana Loki:

```
# Kibana KQL
Properties.TenantId: "acme-corp"

# Loki LogQL
{service="techspherex-api"} |= "acme-corp"
```
