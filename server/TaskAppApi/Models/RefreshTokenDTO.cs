namespace TaskAppApi.Models;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RefreshTokenResponse(string? AccessToken = null, string? RefreshToken = null, bool Success = true);