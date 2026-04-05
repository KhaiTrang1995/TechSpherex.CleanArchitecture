# Native AOT Guide

## Overview

**Native AOT (Ahead-of-Time)** compilation produces a self-contained native executable that doesn't require the .NET runtime. This results in:

| Metric | JIT | Native AOT |
|--------|-----|------------|
| Startup time | ~500ms | ~50ms |
| Memory usage | ~100 MB | ~30 MB |
| Docker image | ~120 MB | ~35 MB |
| Build time | Fast | Slower (native compilation) |
| Compatibility | Full | Some limitations |

## When to Use AOT

✅ **Use AOT when:**
- Deploying to serverless (AWS Lambda, Azure Functions)
- Running in resource-constrained environments
- Startup time is critical (autoscaling, cold starts)
- Building CLI tools or microservices

❌ **Avoid AOT when:**
- Using heavy reflection (EF Core migrations at runtime)
- Rapid development iteration
- Using libraries without AOT support

## Building with AOT

### CLI

```bash
dotnet publish src/Api/TechSpherex.CleanArchitecture.Api.csproj \
  -c Release \
  /p:PublishAot=true \
  /p:StripSymbols=true
```

### Docker

```bash
docker build --build-arg PUBLISH_AOT=true -t techspherex-api:aot .
```

## AOT Compatibility

### ✅ Fully Compatible
- Minimal APIs with TypedResults
- JWT authentication
- Health checks
- ProblemDetails
- Manual CQRS handlers (no reflection at runtime)

### ⚠️ Requires Source Generators
- JSON serialization → Use `JsonSerializerContext`
- FluentValidation → Works but needs trimming configuration

### ❌ Not Compatible (Run Before AOT)
- EF Core migrations → Run migrations separately
- Identity seed data → Use migration scripts or init containers
- Assembly scanning for handlers → Pre-register at build time

## Source-Generated JSON

For AOT, you need source-generated JSON serialization. Create a `JsonContext.cs`:

```csharp
using System.Text.Json.Serialization;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Create;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Get;
// ... other imports

[JsonSerializable(typeof(CreateTodoCommand))]
[JsonSerializable(typeof(CreateTodoResponse))]
[JsonSerializable(typeof(TodoDetailResponse))]
// ... register all request/response types
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonContext : JsonSerializerContext;
```

Then register in `Program.cs`:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});
```

## AOT + EF Core Strategy

Since EF Core migrations can't run in AOT mode, use this approach:

### Option 1: Init Container (Kubernetes)
```yaml
initContainers:
  - name: migration
    image: techspherex-api:latest  # JIT image
    command: ["dotnet", "ef", "database", "update"]
```

### Option 2: Migration Script
```bash
# Generate SQL script
dotnet ef migrations script --idempotent -o migration.sql

# Apply in CI/CD
psql -h $DB_HOST -U $DB_USER -d $DB_NAME -f migration.sql
```

### Option 3: Startup Migration (JIT only)
The default template already does this in `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    await AppDbSeeder.SeedAsync(app.Services);
}
```

## Production Recommendations

1. **Use JIT for development**, AOT for production images
2. **Pre-generate migrations** as SQL scripts in CI/CD
3. **Test AOT builds in CI** to catch trim warnings early
4. **Monitor trim warnings** — they indicate potential runtime failures
