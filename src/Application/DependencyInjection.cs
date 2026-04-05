
using System.Reflection;
using TechSpherex.CleanArchitecture.Application.Abstractions.Agents;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Application.Features.Agents;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace TechSpherex.CleanArchitecture.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddHandlersFromAssembly(assembly);
        services.AddSkillAgents(assembly);

        return services;
    }

    private static void AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceTypes = new[]
        {
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>)
        };

        var types = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            handlerInterfaceTypes.Contains(i.GetGenericTypeDefinition()));

            foreach (var handlerInterface in interfaces)
            {
                services.AddScoped(handlerInterface, type);
            }
        }
    }

    private static void AddSkillAgents(this IServiceCollection services, Assembly assembly)
    {
        // Register all ISkillAgent implementations
        var skillTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(ISkillAgent).IsAssignableFrom(t));

        foreach (var skillType in skillTypes)
        {
            services.AddScoped(typeof(ISkillAgent), skillType);
        }

        // Register the orchestrator
        services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
    }
}
