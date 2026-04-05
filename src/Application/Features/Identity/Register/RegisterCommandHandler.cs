
using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Domain.Common;
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace TechSpherex.CleanArchitecture.Application.Features.Identity.Register;

public sealed class RegisterCommandHandler(UserManager<ApplicationUser> userManager) : ICommandHandler<RegisterCommand>
{
    public async Task<Result> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
            return Result.Failure(Error.Conflict("Auth.EmailTaken", "A user with this email already exists."));

        var user = new ApplicationUser
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            UserName = command.Email
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Auth.RegistrationFailed", errors));
        }

        return Result.Success();
    }
}
