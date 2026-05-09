using System.Security.Cryptography;
using System.Text;

namespace NetProject.Application.Auth;

public static class RefreshTokenGenerator
{
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string Sha256(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

