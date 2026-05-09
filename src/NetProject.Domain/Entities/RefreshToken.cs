using NetProject.Domain.Common;

namespace NetProject.Domain.Entities;

public sealed class RefreshToken : EntityBase
{
    public required string TokenHash { get; init; }
    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public DateTimeOffset? RevokedAtUtc { get; set; }
    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;

    public Guid UserId { get; init; }
    public User? User { get; init; }
}

