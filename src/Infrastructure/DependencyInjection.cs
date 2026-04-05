
using System.Text;
using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Application.Abstractions.Identity;
using TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;
using TechSpherex.CleanArchitecture.Domain.Entities;
using TechSpherex.CleanArchitecture.Infrastructure.Identity;
using TechSpherex.CleanArchitecture.Infrastructure.Persistence;
using TechSpherex.CleanArchitecture.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
namespace TechSpherex.CleanArchitecture.Infrastructure;

// Copyright (c) 2026 TechSpherex
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence();
        services.AddAuth(configuration);
        services.AddCachingServices();
        services.AddMultiTenancy();

        return services;
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }

    private static void AddCachingServices(this IServiceCollection services)
    {
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
    }

    private static void AddMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
    }
}
