using NetProject.Application.Auth.Dtos;

namespace NetProject.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
}

