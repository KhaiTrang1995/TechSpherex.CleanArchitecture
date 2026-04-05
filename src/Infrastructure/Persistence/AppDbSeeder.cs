
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace TechSpherex.CleanArchitecture.Infrastructure.Persistence;

public static class AppDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager, logger);
        await SeedAdminUserAsync(userManager, logger);
        await SeedSampleTodosAsync(context, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@TechSpherex.dev";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
    }

    private static async Task SeedSampleTodosAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Todos.AnyAsync())
            return;

        var todos = new List<TodoItem>
        {
            new() { Title = "Explore the Clean Architecture template", Description = "Read through the layers: Domain → Application → Infrastructure → Api" },
            new() { Title = "Run the API with Aspire", Description = "Use 'dotnet run' in the AppHost project to start PostgreSQL, Redis, and the API" },
            new() { Title = "Try the Scalar API docs", Description = "Navigate to /scalar/v1 to explore and test the endpoints" },
            new() { Title = "Add your first feature", Description = "Create a new entity, command/query handlers, and endpoint following the Todos pattern" },
            new() { Title = "Check the architecture tests", Description = "Run 'dotnet test' to verify dependency rules are enforced" }
        };

        context.Todos.AddRange(todos);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample todos", todos.Count);
    }
}
