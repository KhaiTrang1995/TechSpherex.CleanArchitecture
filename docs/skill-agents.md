# Skill Agents Guide

## Overview

The **Skill Agents** pattern provides a structured way to integrate AI capabilities into your Clean Architecture application. Each "skill" is a discrete capability (e.g., manage todos, generate reports) that can be invoked via natural language prompts.

## Architecture

```
                     POST /api/agents/execute
                            │
                            ▼
                    ┌───────────────┐
                    │  Orchestrator  │  ← Routes prompt to skill
                    └───────┬───────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
        ┌──────────┐ ┌──────────┐ ┌──────────┐
        │   Todo   │ │  Report  │ │  Custom  │
        │  Skill   │ │  Skill   │ │  Skill   │
        └──────────┘ └──────────┘ └──────────┘
              │             │             │
              ▼             ▼             ▼
         Existing      Existing      Your own
         CQRS          Services      logic
         Handlers
```

## Key Components

### ISkillAgent
```csharp
public interface ISkillAgent
{
    string SkillId { get; }
    string Name { get; }
    string Description { get; }
    IReadOnlyList<string> ExamplePrompts { get; }
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct);
}
```

### AgentContext
Contains the prompt, parameters, user/tenant context, and conversation history.

### AgentResult
Structured response with status (`Success`, `Failure`, `NeedsMoreInfo`, `PartialSuccess`), message, and data.

### IAgentOrchestrator
Routes prompts to the appropriate skill agent. The default implementation uses keyword matching. **Replace with LLM-based routing in production.**

## API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/agents/execute` | POST | Yes | Execute with auto-routing |
| `/api/agents/execute/{skillId}` | POST | Yes | Execute specific skill |
| `/api/agents/skills` | GET | No | List available skills |

### Example: Execute Agent

```bash
curl -X POST https://localhost:7200/api/agents/execute \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Show me all my todos",
    "parameters": {},
    "history": []
  }'
```

**Response:**
```json
{
  "status": "Success",
  "message": "Found 5 todos:",
  "data": {
    "todos": [
      "1. [ ] Explore the Clean Architecture template",
      "2. [ ] Run the API with Aspire",
      "3. [✓] Add your first feature"
    ],
    "total": 5
  },
  "metadata": {}
}
```

## Creating a New Skill Agent

### Step 1: Create the Skill Class

```csharp
namespace TechSpherex.CleanArchitecture.Application.Features.Agents.Skills;

using TechSpherex.CleanArchitecture.Application.Abstractions.Agents;

public sealed class ReportAgentSkill : ISkillAgent
{
    private readonly IAppDbContext _dbContext;

    public ReportAgentSkill(IAppDbContext dbContext) => _dbContext = dbContext;

    public string SkillId => "report-generator";
    public string Name => "Report Generator";
    public string Description => "Generate reports and analytics from your data.";

    public IReadOnlyList<string> ExamplePrompts =>
    [
        "Generate a daily report",
        "Show me task completion stats",
        "What's the productivity trend?"
    ];

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct)
    {
        // Your logic here
        var totalTodos = await _dbContext.Todos.CountAsync(ct);
        var completed = await _dbContext.Todos.CountAsync(t => t.IsCompleted, ct);

        return AgentResult.Success(
            $"Report: {completed}/{totalTodos} tasks completed ({(double)completed/totalTodos:P0})",
            new { Total = totalTodos, Completed = completed, Rate = (double)completed / totalTodos });
    }
}
```

### Step 2: That's It!

The skill is **auto-discovered** via assembly scanning in `DependencyInjection.cs`. No manual registration needed.

## Integrating with LLM Providers

The default orchestrator uses keyword matching. For production, integrate with an LLM:

### Option A: OpenAI Function Calling

```csharp
public sealed class OpenAIOrchestrator : IAgentOrchestrator
{
    private readonly IEnumerable<ISkillAgent> _skills;
    private readonly HttpClient _httpClient;

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct)
    {
        // 1. Build function definitions from skills
        var functions = _skills.Select(s => new {
            name = s.SkillId,
            description = s.Description,
            parameters = new { type = "object", properties = new { prompt = new { type = "string" } } }
        });

        // 2. Send to OpenAI with functions
        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", new {
            model = "gpt-4o",
            messages = new[] { new { role = "user", content = context.Prompt } },
            functions
        }, ct);

        // 3. Execute the selected skill
        var functionCall = /* parse response */;
        return await ExecuteSkillAsync(functionCall.Name, context, ct);
    }
}
```

### Option B: Microsoft Semantic Kernel

```csharp
// Register Semantic Kernel
builder.Services.AddKernel()
    .AddOpenAIChatCompletion("gpt-4o", apiKey);

// Create SK-based orchestrator
public sealed class SemanticKernelOrchestrator : IAgentOrchestrator
{
    private readonly Kernel _kernel;
    // ... use SK's planning capabilities to route to skills
}
```

### Option C: Ollama (Local)

```csharp
// docker run -d ollama/ollama
builder.Services.AddHttpClient("ollama", c => c.BaseAddress = new Uri("http://localhost:11434"));
```

## Multi-Turn Conversations

The `AgentContext` includes conversation history for multi-turn interactions:

```bash
curl -X POST https://localhost:7200/api/agents/execute \
  -H "Authorization: Bearer <token>" \
  -d '{
    "prompt": "Mark it as completed",
    "history": [
      { "role": "user", "content": "Show me my todos" },
      { "role": "assistant", "content": "Found 3 todos: 1. Review PR #42..." }
    ]
  }'
```

## Best Practices

1. **Keep skills focused** — One skill per domain concept
2. **Reuse handlers** — Skills should call existing CQRS handlers, not duplicate logic
3. **Handle errors gracefully** — Return `NeedsMoreInfo` instead of failing silently
4. **Log skill executions** — The orchestrator logs all executions with skill ID and status
5. **Test skills independently** — Each skill should have unit tests
6. **Version your skills** — Use `SkillId` with version suffix for breaking changes
