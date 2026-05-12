using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService,
    UserManager<User> userManager) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<IResult> Login([FromBody] LoginUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Email and password are required.");
        }

        logger.LogInformation("Login requested from user {Email}", request.Email);
        var response = await authService.LoginUser(request);
        return response.Success ? Results.Ok(new { response.AccessToken, response.RefreshToken, response.User }) : Results.Unauthorized();
    }

    [HttpPost("Register")]
    public async Task<IResult> Register([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return Results.BadRequest("Email, password, first name, and last name are required.");
        }

        logger.LogInformation("Register requested with email {Email}", request.Email);
        var response = await authService.RegisterUser(request);
        return response.Success ? Results.Ok(response.User) : Results.BadRequest(response.ErrorMessages);
    }

    [HttpPost("Refresh")]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.BadRequest("Access token and refresh token are required.");
        }

        logger.LogInformation("Refresh token requested");
        var response = await authService.RefreshToken(request);
        return response.Success ? Results.Ok(new { response.AccessToken, response.RefreshToken }) : Results.UnprocessableEntity();
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IResult> Logout()
    {
        logger.LogInformation("Logout requested");
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            logger.LogWarning("Somehow didn't find user despite authorize");
            return Results.UnprocessableEntity();
        }
        
        var response = await authService.LogoutUser(user.Id.ToString());
        return response.Success ? Results.Ok() : Results.UnprocessableEntity();
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest("User ID and token are required.");
        }

        logger.LogInformation("Confirm email requested");
        var response = await authService.ConfirmEmail(userId, token);
        return response.Success ? Results.Ok() : Results.UnprocessableEntity();
    }
    
    [HttpGet("Me")]
    [Authorize]
    public IResult Me()
    {
        return Results.Ok(User.Claims.ToDictionary(c => c.Type, c => c.Value));
    }
}
