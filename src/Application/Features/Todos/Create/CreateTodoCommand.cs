
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Create;

public sealed record CreateTodoCommand(string Title, string? Description) : ICommand<Result<CreateTodoResponse>>;

public sealed record CreateTodoResponse(Guid Id, string Title, string? Description);
