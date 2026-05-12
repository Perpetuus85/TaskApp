namespace TaskAppApi.Models;

public record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);
public record RegisterUserResponse(User? User = null, bool Success = true, List<string>? ErrorMessages = null);