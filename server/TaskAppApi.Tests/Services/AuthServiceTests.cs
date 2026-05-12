using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskAppApi.Models;
using TaskAppApi.Options;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Services;

public class AuthServiceTests
{
    private const string Secret = "super-secret-key-super-secret-key-12345";

    [Test]
    public async Task LoginUser_WithInvalidCredentials_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var userManager = new TestUserManager();
        userManager.FindByEmailAsyncHandler = _ => Task.FromResult<User?>(null);

        var response = await CreateService(userManager, db).LoginUser(new LoginUserRequest("a@b.com", "bad"));
        Assert.That(response.Success, Is.False);
    }

    [Test]
    public async Task LoginUser_WithValidCredentials_ReturnsTokens()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var userManager = new TestUserManager
        {
            FindByEmailAsyncHandler = _ => Task.FromResult<User?>(user),
            CheckPasswordAsyncHandler = (_, _) => Task.FromResult(true),
            GetRolesAsyncHandler = _ => Task.FromResult<IList<string>>([Roles.Member])
        };

        var response = await CreateService(userManager, db).LoginUser(new LoginUserRequest(user.Email!, "pass"));

        Assert.That(response.Success, Is.True);
        Assert.That(response.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(await db.RefreshTokens.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task LogoutUser_WithInvalidGuid_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var response = await CreateService(new TestUserManager(), db).LogoutUser("not-a-guid");
        Assert.That(response.Success, Is.False);
    }

    [Test]
    public async Task RefreshToken_WithInvalidAccessToken_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var response = await CreateService(new TestUserManager(), db).RefreshToken(new RefreshTokenRequest("invalid", "refresh"));
        Assert.That(response.Success, Is.False);
    }

    [Test]
    public async Task RefreshToken_WithValidData_ReturnsNewTokens()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), Token = "old", UserId = user.Id, Expires = DateTime.UtcNow.AddHours(1) });
        await db.SaveChangesAsync();

        var userManager = new TestUserManager
        {
            FindByIdAsyncHandler = _ => Task.FromResult<User?>(user),
            GetRolesAsyncHandler = _ => Task.FromResult<IList<string>>([Roles.Member])
        };

        var response = await CreateService(userManager, db).RefreshToken(new RefreshTokenRequest(CreateAccessToken(user.Id), "old"));
        Assert.That(response.Success, Is.True);
        Assert.That(response.RefreshToken, Is.Not.EqualTo("old"));
    }

    private static AuthService CreateService(UserManager<User> userManager, AppDbContext db)
    {
        return new AuthService(
            userManager,
            db,
            Microsoft.Extensions.Options.Options.Create(new JwtOptions { SecretKey = Secret, Issuer = "issuer", Audience = "aud", ExpirationInMinutes = 30, RefreshExpirationInDays = 7 }),
            NullLogger<AuthService>.Instance);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@test.com",
        UserName = "user@test.com",
        FirstName = "A",
        LastName = "B"
    };

    private static string CreateAccessToken(Guid userId)
    {
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)), SecurityAlgorithms.HmacSha256),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = "issuer",
            Audience = "aud"
        });
    }

    private sealed class TestUserManager() : UserManager<User>(new StubUserStore(), null, null, null, null, null, null, null, null)
    {
        public Func<string, Task<User?>> FindByEmailAsyncHandler { get; set; } = _ => Task.FromResult<User?>(null);
        public Func<User, string, Task<bool>> CheckPasswordAsyncHandler { get; set; } = (_, _) => Task.FromResult(false);
        public Func<User, Task<IList<string>>> GetRolesAsyncHandler { get; set; } = _ => Task.FromResult<IList<string>>([]);
        public Func<string, Task<User?>> FindByIdAsyncHandler { get; set; } = _ => Task.FromResult<User?>(null);

        public override Task<User?> FindByEmailAsync(string email) => FindByEmailAsyncHandler(email);
        public override Task<bool> CheckPasswordAsync(User user, string password) => CheckPasswordAsyncHandler(user, password);
        public override Task<IList<string>> GetRolesAsync(User user) => GetRolesAsyncHandler(user);
        public override Task<User?> FindByIdAsync(string userId) => FindByIdAsyncHandler(userId);
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
