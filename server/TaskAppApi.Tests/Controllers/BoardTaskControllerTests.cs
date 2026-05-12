using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAppApi.Controllers;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Controllers;

public class BoardTaskControllerTests
{
    [Test]
    public async Task CreateTask_Returns200()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService { CreateResponse = new BoardTask { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Summary = "S", Description = "D", CreatedByUserId = user.Id } };
        var controller = CreateController(service, user);

        var result = await controller.CreateTask(new CreateBoardTaskRequest("S", "D", null, nameof(BoardTaskStatus.ToDo), Guid.NewGuid().ToString()));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task CreateTask_WhenRequestInvalid_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService();
        var controller = CreateController(service, user);

        var result = await controller.CreateTask(new CreateBoardTaskRequest("", "", null, nameof(BoardTaskStatus.ToDo), "not-a-guid"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task UpdateTask_WhenRequestInvalid_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService();
        var controller = CreateController(service, user);

        var result = await controller.UpdateTask(new UpdateBoardTaskRequest("", "", "", null, nameof(BoardTaskStatus.ToDo)));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task DeleteTask_CallsServiceAndReturns200()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService();
        var controller = CreateController(service, user);

        var result = await controller.DeleteTask(new DeleteBoardTaskRequest(Guid.NewGuid().ToString()));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(service.DeleteCalled, Is.True);
    }

    [Test]
    public async Task DeleteTask_WhenRequestInvalid_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService();
        var controller = CreateController(service, user);

        var result = await controller.DeleteTask(new DeleteBoardTaskRequest("not-a-guid"));
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task GetTask_WhenIdInvalid_Returns400()
    {
        var user = CreateUser();
        var service = new StubBoardTaskService();
        var controller = CreateController(service, user);

        var result = await controller.GetTask(Guid.Empty);
        Assert.That(((IStatusCodeHttpResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    private static BoardTaskController CreateController(IBoardTaskService service, User user)
    {
        var userManager = new TestUserManager { GetUserAsyncHandler = _ => Task.FromResult<User?>(user) };
        return new BoardTaskController(NullLogger<BoardTaskController>.Instance, service, userManager)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())])) }
            }
        };
    }

    private static User CreateUser() => new() { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "a@b.com", FirstName = "A", LastName = "B" };

    private sealed class StubBoardTaskService : IBoardTaskService
    {
        public BoardTask? CreateResponse { get; set; }
        public BoardTask? UpdateResponse { get; set; }
        public BoardTask? GetResponse { get; set; }
        public bool DeleteCalled { get; private set; }
        public Task<BoardTask?> CreateTask(User user, CreateBoardTaskRequest createBoardTaskRequest) => Task.FromResult(CreateResponse);
        public Task<BoardTask?> UpdateTask(User user, UpdateBoardTaskRequest updateBoardTaskRequest) => Task.FromResult(UpdateResponse);
        public Task DeleteTask(User user, DeleteBoardTaskRequest deleteBoardTaskRequest) { DeleteCalled = true; return Task.CompletedTask; }
        public Task<BoardTask?> GetTask(User user, Guid id) => Task.FromResult(GetResponse);
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
