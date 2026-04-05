
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TechSpherex.CleanArchitecture.Application.Abstractions.Identity;
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TechSpherex.CleanArchitecture.Infrastructure.Identity;
public sealed class TokenService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ITokenService
{
    public async Task<TokenResponse> GenerateTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(configuration["Jwt:ExpirationInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(double.Parse(configuration["Jwt:RefreshTokenExpirationInDays"]!));
        await userManager.UpdateAsync(user);

        return new TokenResponse(accessToken, refreshToken, expiresAt);
    }

    // Copyright (c) 2026 TechSpherex
    public async Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("Invalid access token.");

        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new InvalidOperationException("User not found.");

        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        return await GenerateTokenAsync(user, cancellationToken);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidOperationException("Invalid token.");

        return principal;
    }

    // Copyright (c) 2026 TechSpherex
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
