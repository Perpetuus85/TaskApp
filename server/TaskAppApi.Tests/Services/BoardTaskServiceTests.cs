using Microsoft.EntityFrameworkCore;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Services;

public class BoardTaskServiceTests
{
    [Test]
    public async Task CreateTask_WhenBoardNotOwned_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        db.Boards.Add(new Board { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Name = "Other" });
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        var result = await service.CreateTask(
            user,
            new CreateBoardTaskRequest("S", "D", null, nameof(BoardTaskStatus.ToDo), db.Boards.First().Id.ToString()));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateTask_WhenBoardOwned_PersistsTask()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var boardId = Guid.NewGuid();
        db.Boards.Add(new Board { Id = boardId, OwnerId = user.Id, Name = "Mine" });
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        var result = await service.CreateTask(
            user,
            new CreateBoardTaskRequest("S", "D", null, nameof(BoardTaskStatus.ToDo), boardId.ToString()));

        Assert.That(result, Is.Not.Null);
        Assert.That(await db.BoardTasks.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateTask_WhenNotOwned_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var task = new BoardTask { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Summary = "S", Description = "D", CreatedByUserId = Guid.NewGuid() };
        db.BoardTasks.Add(task);
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        var result = await service.UpdateTask(
            user,
            new UpdateBoardTaskRequest(task.Id.ToString(), "S2", "D2", null, nameof(BoardTaskStatus.ToDo)));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateTask_WhenOwned_UpdatesValues()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var task = new BoardTask { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Summary = "S", Description = "D", CreatedByUserId = user.Id };
        db.BoardTasks.Add(task);
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        var dueDate = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var result = await service.UpdateTask(
            user,
            new UpdateBoardTaskRequest(task.Id.ToString(), "S2", "D2", dueDate, nameof(BoardTaskStatus.InProgress)));

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Summary, Is.EqualTo("S2"));
        Assert.That(result.Description, Is.EqualTo("D2"));
        Assert.That(result.Status, Is.EqualTo(BoardTaskStatus.InProgress));
        Assert.That(result.DueAt, Is.Not.Null);
    }

    [Test]
    public async Task DeleteTask_WhenOwned_RemovesTask()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var task = new BoardTask { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Summary = "S", Description = "D", CreatedByUserId = user.Id };
        db.BoardTasks.Add(task);
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        await service.DeleteTask(user, new DeleteBoardTaskRequest(task.Id.ToString()));

        Assert.That(await db.BoardTasks.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetTask_WhenNotOwned_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var task = new BoardTask { Id = Guid.NewGuid(), BoardId = Guid.NewGuid(), Summary = "S", Description = "D", CreatedByUserId = Guid.NewGuid() };
        db.BoardTasks.Add(task);
        await db.SaveChangesAsync();

        var service = new BoardTaskService(db);
        var result = await service.GetTask(user, task.Id);

        Assert.That(result, Is.Null);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "a@b.com",
            FirstName = "A",
            LastName = "B"
        };
    }
}
