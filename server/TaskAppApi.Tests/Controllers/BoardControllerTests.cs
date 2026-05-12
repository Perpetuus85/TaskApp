using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAppApi.Controllers;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Controllers;

public class BoardControllerTests
{
    [Test]
    public async Task GetAllBoardsForUser_Returns200()
    {
        var user = CreateUser();
        var service = new StubBoardService { Boards = [new Board { Id = Guid.NewGuid(), OwnerId = user.Id, Name = "Board" }] };
        var controller = CreateController(service, user);

        var result = await controller.GetAllBoardsForUser();
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task CreateBoard_WhenServiceFails_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardService { CreateResponse = new CreateBoardResponse(false, "error") };
        var controller = CreateController(service, user);

        var result = await controller.CreateBoard(new CreateBoardRequest("X"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task CreateBoard_WhenNameMissing_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardService();
        var controller = CreateController(service, user);

        var result = await controller.CreateBoard(new CreateBoardRequest(" "));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task UpdateBoard_WhenBoardFound_Returns200()
    {
        var user = CreateUser();
        var boardId = Guid.NewGuid();
        var service = new StubBoardService
        {
            UpdateResponse = new Board { Id = boardId, OwnerId = user.Id, Name = "Updated" }
        };
        var controller = CreateController(service, user);

        var result = await controller.UpdateBoard(new UpdateBoardRequest(boardId.ToString(), "Updated"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task UpdateBoard_WhenBoardMissing_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardService { UpdateResponse = null };
        var controller = CreateController(service, user);

        var result = await controller.UpdateBoard(new UpdateBoardRequest(Guid.NewGuid().ToString(), "Updated"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task UpdateBoard_WhenBoardIdMissing_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardService();
        var controller = CreateController(service, user);

        var result = await controller.UpdateBoard(new UpdateBoardRequest(string.Empty, "Updated"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task UpdateBoard_WhenNameMissing_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardService();
        var controller = CreateController(service, user);

        var result = await controller.UpdateBoard(new UpdateBoardRequest(Guid.NewGuid().ToString(), " "));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    private static BoardController CreateController(IBoardService service, User user)
    {
        var userManager = new TestUserManager { GetUserAsyncHandler = _ => Task.FromResult<User?>(user) };
        return new BoardController(NullLogger<BoardController>.Instance, service, userManager)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())])) }
            }
        };
    }

    private static User CreateUser() => new() { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "a@b.com", FirstName = "A", LastName = "B" };

    private sealed class StubBoardService : IBoardService
    {
        public List<Board> Boards { get; set; } = [];
        public CreateBoardResponse CreateResponse { get; set; } = new();
        public Board? BoardResponse { get; set; }
        public Board? UpdateResponse { get; set; }
        public Task<List<Board>> GetAllBoardsForUser(Guid userId) => Task.FromResult(Boards);
        public Task<CreateBoardResponse> CreateBoard(User user, CreateBoardRequest request) => Task.FromResult(CreateResponse);
        public Task<Board?> GetBoardWithTasks(User user, Guid id) => Task.FromResult(BoardResponse);
        public Task<Board?> UpdateBoard(User user, UpdateBoardRequest request) => Task.FromResult(UpdateResponse);
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
