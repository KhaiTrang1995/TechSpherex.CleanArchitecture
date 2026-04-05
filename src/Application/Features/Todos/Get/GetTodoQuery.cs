
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Get;

public sealed record GetTodoQuery(Guid Id) : IQuery<Result<TodoDetailResponse>>;

public sealed record TodoDetailResponse(Guid Id, string Title, string? Description, bool IsCompleted, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt);
