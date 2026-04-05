
using TechSpherex.CleanArchitecture.Application.Abstractions.Agents;
using TechSpherex.CleanArchitecture.Application.Abstractions.Identity;
using TechSpherex.CleanArchitecture.Application.Abstractions.Tenancy;
namespace TechSpherex.CleanArchitecture.Api.Endpoints;

// Copyright (c) 2026 TechSpherex
public static class AgentEndpoints
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agents")
            .WithTags("Skill Agents")
            .RequireAuthorization();

        group.MapPost("/execute", Execute)
            .WithName("ExecuteAgent")
            .WithSummary("Execute a skill agent with a natural language prompt");

        group.MapPost("/execute/{skillId}", ExecuteSkill)
            .WithName("ExecuteSpecificSkill")
            .WithSummary("Execute a specific skill agent by ID");

        group.MapGet("/skills", ListSkills)
            .WithName("ListSkills")
            .WithSummary("List all available skill agents")
            .AllowAnonymous();
    }

    private static async Task<IResult> Execute(
        AgentExecuteRequest request,
        IAgentOrchestrator orchestrator,
        ICurrentUser currentUser,
        ITenantProvider tenantProvider,
        CancellationToken cancellationToken)
    {
        var context = new AgentContext
        {
            Prompt = request.Prompt,
            Parameters = request.Parameters ?? [],
            UserId = currentUser.UserId,
            TenantId = tenantProvider.TenantId,
            ConversationHistory = request.History?.Select(h =>
                new AgentMessage(h.Role, h.Content, h.Timestamp ?? DateTimeOffset.UtcNow)).ToList() ?? []
        };

        var result = await orchestrator.ExecuteAsync(context, cancellationToken);

        return result.Status switch
        {
            AgentResultStatus.Success => TypedResults.Ok(new AgentExecuteResponse(result.Status.ToString(), result.Message, result.Data, result.Metadata)),
            AgentResultStatus.NeedsMoreInfo => TypedResults.Ok(new AgentExecuteResponse(result.Status.ToString(), result.Message, null, result.Metadata)),
            AgentResultStatus.PartialSuccess => TypedResults.Ok(new AgentExecuteResponse(result.Status.ToString(), result.Message, result.Data, result.Metadata)),
            _ => TypedResults.UnprocessableEntity(new AgentExecuteResponse(result.Status.ToString(), result.Message, null, result.Metadata))
        };
    }

    private static async Task<IResult> ExecuteSkill(
        string skillId,
        AgentExecuteRequest request,
        IAgentOrchestrator orchestrator,
        ICurrentUser currentUser,
        ITenantProvider tenantProvider,
        CancellationToken cancellationToken)
    {
        var context = new AgentContext
        {
            Prompt = request.Prompt,
            Parameters = request.Parameters ?? [],
            UserId = currentUser.UserId,
            TenantId = tenantProvider.TenantId
        };

        var result = await orchestrator.ExecuteSkillAsync(skillId, context, cancellationToken);

        return result.Status == AgentResultStatus.Failure
            ? TypedResults.UnprocessableEntity(new AgentExecuteResponse(result.Status.ToString(), result.Message, null, result.Metadata))
            : TypedResults.Ok(new AgentExecuteResponse(result.Status.ToString(), result.Message, result.Data, result.Metadata));
    }

    private static IResult ListSkills(IAgentOrchestrator orchestrator)
    {
        var skills = orchestrator.GetAvailableSkills();
        return TypedResults.Ok(skills);
    }
}

// ─── DTOs ───────────────────────────────────────────────────
public sealed record AgentExecuteRequest(
    string Prompt,
    Dictionary<string, object?>? Parameters = null,
    List<AgentMessageDto>? History = null);

public sealed record AgentMessageDto(string Role, string Content, DateTimeOffset? Timestamp);

public sealed record AgentExecuteResponse(
    string Status,
    string Message,
    object? Data,
    Dictionary<string, object?>? Metadata);
