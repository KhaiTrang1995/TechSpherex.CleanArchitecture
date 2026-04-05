
using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Create;

public sealed class CreateTodoCommandHandler(IAppDbContext dbContext) : ICommandHandler<CreateTodoCommand, Result<CreateTodoResponse>>
{
    public async Task<Result<CreateTodoResponse>> HandleAsync(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = new TodoItem
        {
            Title = command.Title,
            Description = command.Description
        };

        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateTodoResponse(todo.Id, todo.Title, todo.Description);
        return Result.Success(response);
    }
}
