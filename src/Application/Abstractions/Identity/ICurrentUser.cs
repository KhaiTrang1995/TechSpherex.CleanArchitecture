namespace TechSpherex.CleanArchitecture.Application.Abstractions.Identity;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
