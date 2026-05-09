using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetProject.Application.Abstractions.Security;
using NetProject.Application.Auth;

namespace NetProject.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<AuthOptions> jwtOptions) : IJwtTokenService
{
    private readonly AuthOptions _options = jwtOptions.Value;
    private readonly JwtSecurityTokenHandler _handler = new();

    public string CreateAccessToken(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: creds);

        return _handler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            return _handler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }
}

