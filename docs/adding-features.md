# Adding a New Feature

This guide walks you through adding a complete new feature following the **Todos** pattern. We'll create a **Products** feature as an example.

## Step 1: Domain Entity

Create `src/Domain/Entities/Product.cs`:

```csharp
namespace TechSpherex.CleanArchitecture.Domain.Entities;

using TechSpherex.CleanArchitecture.Domain.Common;

public sealed class Product : AuditableEntity, ITenantEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public string TenantId { get; set; } = default!; // Multi-tenant support
}
```

> **Note**: Implement `ITenantEntity` if the entity should be tenant-isolated. The global query filter will be applied automatically.

## Step 2: Add DbSet

Update `IAppDbContext` in `src/Application/Abstractions/Data/IAppDbContext.cs`:

```csharp
public interface IAppDbContext
{
    DbSet<TodoItem> Todos { get; }
    DbSet<Product> Products { get; }  // Add this
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

Update `AppDbContext` in `src/Infrastructure/Persistence/AppDbContext.cs`:

```csharp
public DbSet<Product> Products => Set<Product>();
```

## Step 3: EF Core Configuration

Create `src/Infrastructure/Persistence/Configurations/ProductConfiguration.cs`:

```csharp
namespace TechSpherex.CleanArchitecture.Infrastructure.Persistence.Configurations;

using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.HasIndex(p => p.TenantId);
    }
}
```

## Step 4: CQRS — Create Command

Create `src/Application/Features/Products/Create/`:

**CreateProductCommand.cs**:
```csharp
namespace TechSpherex.CleanArchitecture.Application.Features.Products.Create;

using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price) : ICommand<Result<CreateProductResponse>>;

public sealed record CreateProductResponse(Guid Id, string Name, decimal Price);
```

**CreateProductValidator.cs**:
```csharp
namespace TechSpherex.CleanArchitecture.Application.Features.Products.Create;

using FluentValidation;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
```

**CreateProductHandler.cs**:
```csharp
namespace TechSpherex.CleanArchitecture.Application.Features.Products.Create;

using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;

public sealed class CreateProductHandler
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    private readonly IAppDbContext _context;

    public CreateProductHandler(IAppDbContext context) => _context = context;

    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProductResponse(product.Id, product.Name, product.Price));
    }
}
```

## Step 5: API Endpoint

Create `src/Api/Endpoints/ProductEndpoints.cs`:

```csharp
namespace TechSpherex.CleanArchitecture.Api.Endpoints;

using TechSpherex.CleanArchitecture.Api.Extensions;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Application.Features.Products.Create;
using TechSpherex.CleanArchitecture.Domain.Common;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateProductCommand>>()
            .WithName("CreateProduct")
            .WithSummary("Create a new product");
    }

    private static async Task<IResult> Create(
        CreateProductCommand command,
        ICommandHandler<CreateProductCommand, Result<CreateProductResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created($"/api/products/{result.Value!.Id}", result.Value)
            : result.ToProblemDetails();
    }
}
```

## Step 6: Register in Program.cs

In `src/Api/Program.cs`, add:

```csharp
app.MapProductEndpoints();
```

## Step 7: Database Migration

```bash
cd src/Api
dotnet ef migrations add AddProduct --project ../Infrastructure
dotnet ef database update
```

## Checklist

- [ ] Domain entity created with proper base class
- [ ] `ITenantEntity` implemented (if multi-tenant)
- [ ] DbSet added to `IAppDbContext` and `AppDbContext`
- [ ] EF Core configuration added
- [ ] CQRS command/query + handler + validator created
- [ ] API endpoint created and registered in `Program.cs`
- [ ] Migration generated and applied
- [ ] Unit tests added
- [ ] Architecture tests still pass
