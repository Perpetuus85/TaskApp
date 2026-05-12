namespace TaskAppApi.Models;

public record LoginUserRequest(string Email, string Password);
public record LoginUserResponse(string? AccessToken = null, string? RefreshToken = null, bool Success = true, User? User = null);