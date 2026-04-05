
using TechSpherex.CleanArchitecture.Application.Features.Todos.Complete;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;
using FluentAssertions;
namespace TechSpherex.CleanArchitecture.Application.UnitTests.Features.Todos;

public sealed class CompleteTodoCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Mark_Todo_As_Completed()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var todo = new TodoItem { Title = "Test" };
        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CompleteTodoCommandHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new CompleteTodoCommand(todo.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await dbContext.Todos.FindAsync([todo.Id], TestContext.Current.CancellationToken);
        updated!.IsCompleted.Should().BeTrue();
        updated.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CompleteTodoCommandHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new CompleteTodoCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
