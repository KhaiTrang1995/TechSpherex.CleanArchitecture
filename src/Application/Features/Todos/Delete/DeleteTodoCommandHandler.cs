
using TechSpherex.CleanArchitecture.Application.Abstractions.Data;
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Delete;

public sealed class DeleteTodoCommandHandler(IAppDbContext dbContext) : ICommandHandler<DeleteTodoCommand>
{
    public async Task<Result> HandleAsync(DeleteTodoCommand command, CancellationToken cancellationToken = default)
    {
        var todo = await dbContext.Todos.FindAsync([command.Id], cancellationToken);
        if (todo is null)
            return Result.Failure(Error.NotFound("Todo.NotFound", $"Todo with ID '{command.Id}' was not found."));

        dbContext.Todos.Remove(todo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
