using System.Security.Claims;

namespace NetProject.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return raw is null ? Guid.Empty : Guid.Parse(raw);
    }
}

