
using System.Linq.Expressions;
using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace TechSpherex.CleanArchitecture.Infrastructure.Persistence;

// Copyright (c) 2026 TechSpherex
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options), IAppDbContext
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Multi-tenant global query filter
        // Applies to all entities implementing ITenantEntity
        var tenantEntityTypes = builder.Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType)
            .Where(clrType => typeof(ITenantEntity).IsAssignableFrom(clrType))
            .ToList();

        foreach (var clrType in tenantEntityTypes)
        {
            // e => e.TenantId == "default", built per entity type without reflection-invoke
            var entity = Expression.Parameter(clrType, "e");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(entity, nameof(ITenantEntity.TenantId)),
                    Expression.Constant("default")),
                entity);

            builder.Entity(clrType).HasQueryFilter(filter);
            builder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        SetTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Copyright (c) 2026 TechSpherex
    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
    }

    private void SetTenantId()
    {
        // Resolve ITenantProvider from the current scope via IServiceProvider
        // This avoids constructor injection which is incompatible with DbContextPooling (Aspire)
        var serviceProvider = this.GetInfrastructure();
        var tenantProvider = serviceProvider.GetService<ITenantProvider>();

        if (tenantProvider?.TenantId is null) return;

        var tenantEntries = ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in tenantEntries)
        {
            entry.Entity.TenantId = tenantProvider.TenantId;
        }
    }
}
