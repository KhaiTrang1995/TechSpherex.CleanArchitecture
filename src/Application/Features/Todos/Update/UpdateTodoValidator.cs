
using FluentValidation;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.Update;

public sealed class UpdateTodoValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
