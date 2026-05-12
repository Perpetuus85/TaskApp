using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAppApi.Controllers;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Controllers;

public class AuthControllerTests
{
    [Test]
    public async Task Login_WhenAuthSucceeds_Returns200()
    {
        var authService = new StubAuthService { LoginResponse = new LoginUserResponse("access", "refresh", true, CreateUser()) };
        var controller = CreateController(authService, _ => { });

        var result = await controller.Login(new LoginUserRequest("a@b.com", "pass"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task Login_WhenRequestInvalid_Returns400()
    {
        var controller = CreateController(new StubAuthService(), _ => { });
        var result = await controller.Login(new LoginUserRequest("", ""));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Register_WhenRequestInvalid_Returns400()
    {
        var controller = CreateController(new StubAuthService(), _ => { });
        var result = await controller.Register(new RegisterUserRequest("", "", "", ""));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Refresh_WhenRequestInvalid_Returns400()
    {
        var controller = CreateController(new StubAuthService(), _ => { });
        var result = await controller.RefreshToken(new RefreshTokenRequest("", ""));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task ConfirmEmail_WhenRequestInvalid_Returns400()
    {
        var controller = CreateController(new StubAuthService(), _ => { });
        var result = await controller.ConfirmEmail("", "");
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Logout_WhenUserNotFound_Returns422()
    {
        var authService = new StubAuthService();
        var controller = CreateController(authService, um => um.GetUserAsyncHandler = _ => Task.FromResult<User?>(null));

        var result = await controller.Logout();
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
    }

    [Test]
    public void Me_ReturnsClaimsAsDictionary()
    {
        var controller = CreateController(new StubAuthService(), _ => { });
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, "a@b.com")], "test"))
            }
        };

        var result = controller.Me();
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        var dict = ((IValueHttpResult)result).Value as Dictionary<string, string>;
        Assert.That(dict, Is.Not.Null);
        Assert.That(dict![ClaimTypes.Email], Is.EqualTo("a@b.com"));
    }

    private static AuthController CreateController(IAuthService authService, Action<TestUserManager> configure)
    {
        var userManager = new TestUserManager();
        configure(userManager);

        return new AuthController(NullLogger<AuthController>.Instance, authService, userManager)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())]))
                }
            }
        };
    }

    private static User CreateUser() => new() { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "a@b.com", FirstName = "A", LastName = "B" };

    private sealed class StubAuthService : IAuthService
    {
        public LoginUserResponse LoginResponse { get; set; } = new(Success: false);
        public RegisterUserResponse RegisterResponse { get; set; } = new(Success: true);
        public RefreshTokenResponse RefreshResponse { get; set; } = new(Success: true);
        public ConfirmEmailResponse ConfirmEmailResponse { get; set; } = new(true);
        public LogoutUserResponse LogoutResponse { get; set; } = new(true);
        public Task<LoginUserResponse> LoginUser(LoginUserRequest userRequest) => Task.FromResult(LoginResponse);
        public Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request) => Task.FromResult(RegisterResponse);
        public Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request) => Task.FromResult(RefreshResponse);
        public Task<ConfirmEmailResponse> ConfirmEmail(string userId, string token) => Task.FromResult(ConfirmEmailResponse);
        public Task<LogoutUserResponse> LogoutUser(string userId) => Task.FromResult(LogoutResponse);
    }

    private sealed class TestUserManager() : UserManager<User>(new StubUserStore(), null!, null!, null!, null!, null!, null!, null!, null!)
    {
        public Func<ClaimsPrincipal, Task<User?>> GetUserAsyncHandler { get; set; } = _ => Task.FromResult<User?>(null);
        public override Task<User?> GetUserAsync(ClaimsPrincipal principal) => GetUserAsyncHandler(principal);
    }

    private sealed class StubUserStore : IUserStore<User>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken) { user.UserName = userName; return Task.CompletedTask; }
        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<User?>(null);
        public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<User?>(null);
    }
}
