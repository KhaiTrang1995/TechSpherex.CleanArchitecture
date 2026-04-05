
using TechSpherex.CleanArchitecture.Application.Abstractions.Agents;
using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace TechSpherex.CleanArchitecture.Application.Features.Agents.Skills;

/// <summary>
/// Sample skill agent that manages Todos through natural language commands.
/// Demonstrates how to integrate skill agents with existing CQRS handlers.
/// </summary>
public sealed class TodoAgentSkill : ISkillAgent
{
    private readonly IAppDbContext _dbContext;

    public TodoAgentSkill(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string SkillId => "todo-manager";
    public string Name => "Todo Manager";
    public string Description => "Manage todo items — create, list, complete, and delete todos using natural language.";

    public IReadOnlyList<string> ExamplePrompts =>
    [
        "Show me all my todos",
        "Create a todo: Review PR #42",
        "Mark todo as completed",
        "Delete all completed todos",
        "How many todos do I have?"
    ];

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        var prompt = context.Prompt.ToLowerInvariant().Trim();

        // Simple intent detection (in production, use an LLM for this)
        return prompt switch
        {
            var p when p.Contains("list") || p.Contains("show") || p.Contains("all") =>
                await ListTodosAsync(cancellationToken),

            var p when p.Contains("create") || p.Contains("add") || p.Contains("new") =>
                await CreateTodoAsync(context, cancellationToken),

            var p when p.Contains("complete") || p.Contains("done") || p.Contains("finish") =>
                await CompleteTodoAsync(context, cancellationToken),

            var p when p.Contains("delete") || p.Contains("remove") =>
                await DeleteCompletedAsync(cancellationToken),

            var p when p.Contains("count") || p.Contains("how many") =>
                await CountTodosAsync(cancellationToken),

            _ => AgentResult.NeedsMoreInfo(
                "I can help you manage todos. Try:\n" +
                "- 'Show all todos'\n" +
                "- 'Create a todo: <title>'\n" +
                "- 'Complete todo: <title>'\n" +
                "- 'Delete completed todos'\n" +
                "- 'How many todos do I have?'")
        };
    }

    private async Task<AgentResult> ListTodosAsync(CancellationToken ct)
    {
        var todos = await _dbContext.Todos.OrderByDescending(t => t.CreatedAt).Take(20).ToListAsync(ct);

        if (todos.Count == 0)
            return AgentResult.Success("You have no todos. Use 'Create a todo: <title>' to add one.");

        var list = todos.Select((t, i) =>
            $"{i + 1}. [{(t.IsCompleted ? "✓" : " ")}] {t.Title}").ToList();

        return AgentResult.Success(
            $"Found {todos.Count} todos:",
            new { Todos = list, Total = todos.Count });
    }

    private async Task<AgentResult> CreateTodoAsync(AgentContext context, CancellationToken ct)
    {
        // Extract title after "create" or "add" keyword
        var prompt = context.Prompt;
        var colonIndex = prompt.IndexOf(':');
        var title = colonIndex >= 0 
            ? prompt[(colonIndex + 1)..].Trim()
            : prompt.Split(' ').Length > 2
                ? string.Join(' ', prompt.Split(' ').Skip(2))
                : null;

        if (string.IsNullOrWhiteSpace(title))
            return AgentResult.NeedsMoreInfo("Please provide a title. Example: 'Create a todo: Review PR #42'");

        var todo = new TodoItem { Title = title };
        _dbContext.Todos.Add(todo);
        await _dbContext.SaveChangesAsync(ct);

        return AgentResult.Success(
            $"Created todo: \"{title}\"",
            new { Id = todo.Id, Title = title });
    }

    private async Task<AgentResult> CompleteTodoAsync(AgentContext context, CancellationToken ct)
    {
        var prompt = context.Prompt;
        var colonIndex = prompt.IndexOf(':');
        var search = colonIndex >= 0 ? prompt[(colonIndex + 1)..].Trim() : null;

        if (string.IsNullOrWhiteSpace(search))
            return AgentResult.NeedsMoreInfo("Please specify which todo to complete. Example: 'Complete todo: Review PR #42'");

        var todo = await _dbContext.Todos
            .FirstOrDefaultAsync(t => t.Title.ToLower().Contains(search.ToLower()) && !t.IsCompleted, ct);

        if (todo is null)
            return AgentResult.Failure($"Could not find an incomplete todo matching: \"{search}\"");

        todo.MarkAsCompleted();
        await _dbContext.SaveChangesAsync(ct);

        return AgentResult.Success($"Completed: \"{todo.Title}\" ✓");
    }

    private async Task<AgentResult> DeleteCompletedAsync(CancellationToken ct)
    {
        var completed = await _dbContext.Todos.Where(t => t.IsCompleted).ToListAsync(ct);

        if (completed.Count == 0)
            return AgentResult.Success("No completed todos to delete.");

        _dbContext.Todos.RemoveRange(completed);
        await _dbContext.SaveChangesAsync(ct);

        return AgentResult.Success($"Deleted {completed.Count} completed todo(s).");
    }

    private async Task<AgentResult> CountTodosAsync(CancellationToken ct)
    {
        var total = await _dbContext.Todos.CountAsync(ct);
        var completed = await _dbContext.Todos.CountAsync(t => t.IsCompleted, ct);
        var pending = total - completed;

        return AgentResult.Success(
            $"You have {total} todos: {pending} pending, {completed} completed.",
            new { Total = total, Pending = pending, Completed = completed });
    }
}
