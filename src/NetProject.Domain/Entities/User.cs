using NetProject.Domain.Common;

namespace NetProject.Domain.Entities;

public sealed class User : EntityBase
{
    public required string Email { get; init; }
    public required string PasswordHash { get; set; }

    public List<RefreshToken> RefreshTokens { get; init; } = new();
}

