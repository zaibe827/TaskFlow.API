namespace NetProject.Application.Common.Errors;

public sealed class AppException(string message) : Exception(message)
{
    public static AppException InvalidCredentials() => new("Invalid credentials.");
    public static AppException EmailAlreadyInUse() => new("Email is already in use.");
    public static AppException InvalidRefreshToken() => new("Invalid refresh token.");
}

