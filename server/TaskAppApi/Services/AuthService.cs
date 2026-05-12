using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskAppApi.Models;
using TaskAppApi.Options;

namespace TaskAppApi.Services;

public class AuthService(
    UserManager<User> userManager,
    AppDbContext dbContext,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<LoginUserResponse> LoginUser(LoginUserRequest userRequest)
    {
        logger.LogInformation("Attempting to login user with email {Email}", userRequest.Email);
        
        var user = await userManager.FindByEmailAsync(userRequest.Email);

        if (user is null ||
            !await userManager.CheckPasswordAsync(user, userRequest.Password))
        {
            logger.LogInformation("User with email {Email} doesn't exist or invalid credentials given", userRequest.Email);
            return new LoginUserResponse(Success: false);
        }

        var tokenDict = await GenerateTokens(user);
        if (tokenDict is null)
        {
            return new LoginUserResponse(Success: false);
        }
        
        logger.LogInformation("Successfully logged in user with email {Email}", user.Email);
        return new LoginUserResponse
        (
            tokenDict["AccessToken"],
            tokenDict["RefreshToken"],
            User: user
        );
    }

    public async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request)
    {
        logger.LogInformation("Attempting to register user with email {Email}", request.Email);
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName =  request.Email,
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return new RegisterUserResponse(Success: false, ErrorMessages: identityResult.Errors.Select(e => e.Description).ToList());
        }
        
        var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            return new RegisterUserResponse(Success: false, ErrorMessages: addToRoleResult.Errors.Select(e => e.Description).ToList());
        }

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while adding new user");
            return new RegisterUserResponse(Success: false, ErrorMessages: [e.Message]);
        }
        
        logger.LogInformation("Successfully registered user with email {Email}", request.Email);
        
        logger.LogInformation("Creating email confirmation token");
        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        logger.LogInformation("Email confirmation token generated {Token}", emailToken);
        
        // TODO: Send email with link
        
        return new RegisterUserResponse(User: user, Success: true);
    }

    public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request)
    {
        logger.LogInformation("Attempting to refresh access token {AccessToken} with refresh token {RefreshToken}", request.AccessToken, request.RefreshToken);
        // Get user from Token
        var tokenHandler = new JsonWebTokenHandler();
        var tokenValidationResult = await tokenHandler.ValidateTokenAsync(request.AccessToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
        });
        if (!tokenValidationResult.IsValid)
        {
            logger.LogWarning("Invalid access token from request with exception {Exception}", tokenValidationResult.Exception.Message);
            return new RefreshTokenResponse(Success: false);
        }
        
        var userId = (string)tokenValidationResult.Claims[JwtRegisteredClaimNames.Sub];
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return new RefreshTokenResponse(Success: false);
        }
        
        logger.LogInformation("User with id {UserId} obtained from access token", userGuid.ToString());

        var user = await userManager.FindByIdAsync(userGuid.ToString());
        if (user == null)
        {
            logger.LogWarning("User with id {Id} from refresh token does not exist", userGuid.ToString());
            return new RefreshTokenResponse(Success: false);
        }
        
        // Get Refresh token from db using userId
        var dbRefreshToken = dbContext.RefreshTokens.FirstOrDefault(r => r.UserId == userGuid && r.Token == request.RefreshToken);
        if (dbRefreshToken == null)
        {
            logger.LogInformation("Input refresh token {RefreshToken} not valid for user with Id {Id}", request.RefreshToken, userGuid.ToString());
            return new RefreshTokenResponse(Success: false);
        }
        
        // Check refresh token expiry
        if (dbRefreshToken.Expires < DateTime.UtcNow)
        {
            logger.LogInformation("Refresh token expired, new login required");
            return new RefreshTokenResponse(Success: false);
        }
        
        logger.LogInformation("Refresh token valid, removing current refresh token, and generating new access and refresh tokens");
        dbContext.RefreshTokens.Remove(dbRefreshToken);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while removing current refresh token");
            return new RefreshTokenResponse(Success: false);
        }

        var tokenDict = await GenerateTokens(user);
        return tokenDict is null ? new RefreshTokenResponse(Success: false) :
            new RefreshTokenResponse(AccessToken: tokenDict["AccessToken"], RefreshToken: tokenDict["RefreshToken"]);
    }

    public async Task<ConfirmEmailResponse> ConfirmEmail(string userId, string confirmToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ConfirmEmailResponse(Success: false);
        }
        
        var result = await userManager.ConfirmEmailAsync(user, confirmToken);
        if (!result.Succeeded)
        {
            logger.LogWarning("Unable to confirm email for user with Id {Id} and confirm token {Token}", userId, confirmToken);
            return new ConfirmEmailResponse(Success: false);
        }
        
        logger.LogInformation("Email confirmed for user with id {Id}", userId);
        
        return new ConfirmEmailResponse();
    }

    public async Task<LogoutUserResponse> LogoutUser(string userId)
    {
        logger.LogInformation("Attempting to logout user {UserId}", userId);
        if (!Guid.TryParse(userId, out var userGuid))
        {
            logger.LogWarning("Input Id {UserId} is not a GUID", userId);
            return new LogoutUserResponse(Success: false);
        }

        logger.LogInformation("Removing all refresh tokens for user {UserId}", userId);
        dbContext.RefreshTokens.RemoveRange(dbContext.RefreshTokens.Where(r => r.UserId == userGuid));
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while removing current refresh token");
            return new LogoutUserResponse(Success: false);
        }
        
        return new LogoutUserResponse();
    }

    private async Task<Dictionary<string, string>?> GenerateTokens(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateRefreshToken(),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpirationInDays),
        };
        
        dbContext.RefreshTokens.Add(refreshToken);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while adding new user and saving refresh token");
            return null;
        }

        return new Dictionary<string, string>
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken.Token,
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}