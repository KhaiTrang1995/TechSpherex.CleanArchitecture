
using System.Security.Claims;
using TechSpherex.CleanArchitecture.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Http;
namespace TechSpherex.CleanArchitecture.Infrastructure.Identity;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
