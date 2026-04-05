
using TechSpherex.CleanArchitecture.Application.Features.Todos.Get;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;
using FluentAssertions;
namespace TechSpherex.CleanArchitecture.Application.UnitTests.Features.Todos;

public sealed class GetTodoQueryHandlerTests
{
    [Fact]

    // Copyright (c) 2026 TechSpherex
    public async Task HandleAsync_Should_Return_Todo_When_Found()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var todo = new TodoItem { Title = "Test", Description = "Desc" };
        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetTodoQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetTodoQuery(todo.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetTodoQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetTodoQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
