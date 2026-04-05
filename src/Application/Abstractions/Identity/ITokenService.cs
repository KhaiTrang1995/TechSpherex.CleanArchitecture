
using TechSpherex.CleanArchitecture.Domain.Entities;
namespace TechSpherex.CleanArchitecture.Application.Abstractions.Identity;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
}

public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
