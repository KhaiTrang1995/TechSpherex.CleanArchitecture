
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Update;

public sealed record UpdateTodoCommand(Guid Id, string Title, string? Description) : ICommand;
