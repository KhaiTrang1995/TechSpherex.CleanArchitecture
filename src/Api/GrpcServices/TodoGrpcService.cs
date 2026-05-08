using Grpc.Core;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Complete;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Create;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Delete;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Get;
using TechSpherex.CleanArchitecture.Application.Features.Todos.GetAll;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Update;
using TechSpherex.CleanArchitecture.Domain.Common;

namespace TechSpherex.CleanArchitecture.Api.GrpcServices;

/// <summary>
/// gRPC implementation of TodoService.
/// Delegates to the same Application-layer CQRS handlers used by the REST endpoints,
/// ensuring consistent business logic across transport protocols.
/// </summary>
public sealed class TodoGrpcService(
    ICommandHandler<CreateTodoCommand, Result<CreateTodoResponse>> createHandler,
    IQueryHandler<GetTodoQuery, Result<TodoDetailResponse>> getHandler,
    IQueryHandler<GetAllTodosQuery, Result<PagedResult<TodoDetailResponse>>> getAllHandler,
    ICommandHandler<UpdateTodoCommand, Result> updateHandler,
    ICommandHandler<CompleteTodoCommand, Result> completeHandler,
    ICommandHandler<DeleteTodoCommand, Result> deleteHandler)
    : TodoService.TodoServiceBase
{
    public override async Task<TodoResponse> GetTodo(GetTodoRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format."));

        var result = await getHandler.HandleAsync(new GetTodoQuery(id), context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        return MapToResponse(result.Value!);
    }

    public override async Task<TodoListResponse> GetAllTodos(GetAllTodosRequest request, ServerCallContext context)
    {
        var query = new GetAllTodosQuery(
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10);

        var result = await getAllHandler.HandleAsync(query, context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        var response = new TodoListResponse
        {
            TotalCount = result.Value!.TotalCount,
            Page = result.Value.Page,
            PageSize = result.Value.PageSize
        };

        foreach (var item in result.Value.Items)
        {
            response.Items.Add(MapToResponse(item));
        }

        return response;
    }

    public override async Task<TodoResponse> CreateTodo(CreateTodoRequest request, ServerCallContext context)
    {
        var command = new CreateTodoCommand(request.Title, request.HasDescription ? request.Description : null);
        var result = await createHandler.HandleAsync(command, context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        return new TodoResponse
        {
            Id = result.Value!.Id.ToString(),
            Title = result.Value.Title,
            Description = result.Value.Description ?? "",
            IsCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow.ToString("O")
        };
    }

    public override async Task<TodoResponse> UpdateTodo(UpdateTodoRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format."));

        var command = new UpdateTodoCommand(id, request.Title, request.HasDescription ? request.Description : null);
        var result = await updateHandler.HandleAsync(command, context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        // Re-fetch to return updated state
        var getResult = await getHandler.HandleAsync(new GetTodoQuery(id), context.CancellationToken);
        return getResult.IsSuccess ? MapToResponse(getResult.Value!) : new TodoResponse { Id = id.ToString() };
    }

    public override async Task<TodoResponse> CompleteTodo(CompleteTodoRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format."));

        var result = await completeHandler.HandleAsync(new CompleteTodoCommand(id), context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        var getResult = await getHandler.HandleAsync(new GetTodoQuery(id), context.CancellationToken);
        return getResult.IsSuccess ? MapToResponse(getResult.Value!) : new TodoResponse { Id = id.ToString() };
    }

    public override async Task<DeleteTodoResponse> DeleteTodo(DeleteTodoRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format."));

        var result = await deleteHandler.HandleAsync(new DeleteTodoCommand(id), context.CancellationToken);
        if (result.IsFailure)
            throw MapToRpcException(result.Error!);

        return new DeleteTodoResponse { Success = true };
    }

    private static TodoResponse MapToResponse(TodoDetailResponse detail) => new()
    {
        Id = detail.Id.ToString(),
        Title = detail.Title,
        Description = detail.Description ?? "",
        IsCompleted = detail.IsCompleted,
        CreatedAt = detail.CreatedAt.ToString("O"),
        CompletedAt = detail.CompletedAt?.ToString("O") ?? ""
    };

    private static RpcException MapToRpcException(Error error) => error.Type switch
    {
        ErrorType.NotFound => new RpcException(new Status(StatusCode.NotFound, error.Message)),
        ErrorType.Validation => new RpcException(new Status(StatusCode.InvalidArgument, error.Message)),
        ErrorType.Conflict => new RpcException(new Status(StatusCode.AlreadyExists, error.Message)),
        _ => new RpcException(new Status(StatusCode.Internal, error.Message))
    };
}
