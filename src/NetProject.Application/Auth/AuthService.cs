using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetProject.Application.Abstractions.Persistence;
using NetProject.Application.Abstractions.Security;
using NetProject.Application.Abstractions.Time;
using NetProject.Application.Auth.Dtos;
using NetProject.Application.Common.Errors;
using NetProject.Domain.Entities;

namespace NetProject.Application.Auth;

public sealed class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt,
    IDateTimeProvider clock,
    IOptions<AuthOptions> authOptions) : IAuthService
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (exists) throw AppException.EmailAlreadyInUse();

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw AppException.InvalidCredentials();

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenGenerator.Sha256(request.RefreshToken);
        var refreshToken = await db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken?.User is null) throw AppException.InvalidRefreshToken();
        if (refreshToken.IsRevoked || refreshToken.IsExpired) throw AppException.InvalidRefreshToken();

        // Rotate refresh token
        refreshToken.RevokedAtUtc = clock.UtcNow;
        var user = refreshToken.User;

        var response = await IssueTokensAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenGenerator.Sha256(refreshToken);
        var entity = await db.RefreshTokens.SingleOrDefaultAsync(
            x => x.UserId == userId && x.TokenHash == tokenHash,
            cancellationToken);

        if (entity is null) return;
        entity.RevokedAtUtc = clock.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = jwt.CreateAccessToken(user.Id, user.Email);

        var refresh = RefreshTokenGenerator.GenerateToken();
        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = RefreshTokenGenerator.Sha256(refresh),
            ExpiresAtUtc = clock.UtcNow.AddDays(_authOptions.RefreshTokenDays),
        };

        db.RefreshTokens.Add(refreshEntity);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refresh);
    }
}

