using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TechSpherex.CleanArchitecture.Api.Endpoints;
using TechSpherex.CleanArchitecture.Api.Extensions;
using TechSpherex.CleanArchitecture.Api.GrpcServices;
using TechSpherex.CleanArchitecture.Application;
using TechSpherex.CleanArchitecture.Infrastructure;
using TechSpherex.CleanArchitecture.Infrastructure.Persistence;
using TechSpherex.CleanArchitecture.Infrastructure.Tenancy;
using TechSpherex.CleanArchitecture.ServiceDefaults;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Aspire service defaults (OpenTelemetry, health checks, service discovery)
    builder.AddServiceDefaults();

    // Serilog
    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    if (builder.Configuration.GetValue("Database:UseInMemory", false))
    {
        // Demo mode: everything lives in RAM and is lost on shutdown. No PostgreSQL/Redis needed.
        builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TechSpherex"));
        builder.Services.AddDistributedMemoryCache();
    }
    else
    {
        // Aspire-managed PostgreSQL
        builder.AddNpgsqlDbContext<AppDbContext>("TechSpherex-db");

        // Aspire-managed Redis (for HybridCache L2)
        builder.AddRedisDistributedCache("TechSpherex-cache");
    }

    // Application & Infrastructure (includes HybridCache, CORS, RuleEngine)
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // gRPC services
    builder.Services.AddGrpc();

    // Global exception handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // OpenAPI with JWT Bearer security scheme
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var info = document.Info ?? new Microsoft.OpenApi.OpenApiInfo();
            info.Title = "TechSpherex Clean Architecture API";
            info.Description = "A production-ready Clean Architecture template for .NET 11 by TechSpherex";
            info.Contact = new Microsoft.OpenApi.OpenApiContact { Name = "TechSpherex" };
            if (Uri.TryCreate(builder.Configuration["OpenApi:ContactUrl"], UriKind.Absolute, out var contactUrl))
                info.Contact.Url = contactUrl;
            document.Info = info;

            var components = document.Components ?? new Microsoft.OpenApi.OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
            components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            };

            document.Components = components;

            var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
            var securityRequirement = new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };

            document.Security ??= [];
            document.Security.Add(securityRequirement);
            return Task.CompletedTask;
        });
    });

    // ProblemDetails
    builder.Services.AddProblemDetails();

    // HTTP compression. ASP.NET Core 11 adds Zstandard (zstd) next to Brotli and gzip for
    // both response compression and request decompression. Responses over HTTPS are only
    // compressed when ResponseCompression:EnableForHttps is true (off by default: BREACH).
    builder.Services.AddResponseCompression(options =>
        options.EnableForHttps = builder.Configuration.GetValue("ResponseCompression:EnableForHttps", false));
    builder.Services.AddRequestDecompression();

    var app = builder.Build();

    // Global exception handler
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseRequestDecompression();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("TechSpherex Clean Architecture API");
            options.WithTheme(ScalarTheme.BluePlanet);
            options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
        });
    }

    // Multi-tenant middleware (before auth so tenant context is available)
    app.UseMiddleware<TenantMiddleware>();

    // CORS (before auth)
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    // Map REST endpoints
    // Copyright (c) 2026 TechSpherex
    app.MapIdentityEndpoints();
    app.MapTodoEndpoints();
    app.MapAgentEndpoints();

    // Map gRPC services
    app.MapGrpcService<TodoGrpcService>();

    // Aspire default endpoints (health, alive)
    app.MapDefaultEndpoints();

    // Seed database in development
    if (app.Environment.IsDevelopment())
    {
        await AppDbSeeder.SeedAsync(app.Services);
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

