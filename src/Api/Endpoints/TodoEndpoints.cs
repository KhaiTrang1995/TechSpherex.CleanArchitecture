
using TechSpherex.CleanArchitecture.Api.Extensions;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Complete;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Create;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Delete;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Get;
using TechSpherex.CleanArchitecture.Application.Features.Todos.GetAll;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Update;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Api.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("Todos")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllTodos")
            .WithSummary("Get all todos with pagination");

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetTodoById")
            .WithSummary("Get a todo by ID");

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTodoCommand>>()
            .WithName("CreateTodo")
            .WithSummary("Create a new todo");

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTodoRequest>>()
            .WithName("UpdateTodo")
            .WithSummary("Update an existing todo");

        group.MapPatch("/{id:guid}/complete", Complete)
            .WithName("CompleteTodo")
            .WithSummary("Mark a todo as completed");

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteTodo")
            .WithSummary("Delete a todo");
    }

    private static async Task<IResult> GetAll(
        int? page,
        int? pageSize,
        IQueryHandler<GetAllTodosQuery, Result<PagedResult<TodoDetailResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTodosQuery(page ?? 1, pageSize ?? 10);
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetTodoQuery, Result<TodoDetailResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetTodoQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateTodoCommand command,
        ICommandHandler<CreateTodoCommand, Result<CreateTodoResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetTodoById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateTodoRequest request,
        ICommandHandler<UpdateTodoCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoCommand(id, request.Title, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Complete(
        Guid id,
        ICommandHandler<CompleteTodoCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CompleteTodoCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteTodoCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteTodoCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}

public sealed record UpdateTodoRequest(string Title, string? Description);
