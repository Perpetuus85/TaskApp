using TaskAppApi.Models;

namespace TaskAppApi.Services;

public interface IAuthService
{
    Task<LoginUserResponse> LoginUser(LoginUserRequest userRequest);
    Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request);
    Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request);
    Task<ConfirmEmailResponse> ConfirmEmail(string userId, string token);
    Task<LogoutUserResponse> LogoutUser(string userId);
}