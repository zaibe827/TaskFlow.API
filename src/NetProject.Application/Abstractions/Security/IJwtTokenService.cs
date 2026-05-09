using System.Security.Claims;

namespace NetProject.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime);
}

