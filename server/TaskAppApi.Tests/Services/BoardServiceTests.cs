using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Tests.Services;

public class BoardServiceTests
{
    [Test]
    public async Task GetAllBoardsForUser_ReturnsOnlyOwnedBoards()
    {
        await using var db = CreateDbContext();
        var ownerId = Guid.NewGuid();
        db.Boards.AddRange(
            new Board { Id = Guid.NewGuid(), OwnerId = ownerId, Name = "Mine" },
            new Board { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Name = "Other" });
        await db.SaveChangesAsync();

        var service = new BoardService(NullLogger<BoardService>.Instance, db);
        var result = await service.GetAllBoardsForUser(ownerId);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task CreateBoard_SavesBoard()
    {
        await using var db = CreateDbContext();
        var service = new BoardService(NullLogger<BoardService>.Instance, db);

        var response = await service.CreateBoard(CreateUser(), new CreateBoardRequest("New Board"));

        Assert.That(response.Success, Is.True);
        Assert.That(await db.Boards.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateBoard_WhenOwnedBoardExists_UpdatesNameAndReturnsBoard()
    {
        await using var db = CreateDbContext();
        var user = CreateUser();
        var board = new Board { Id = Guid.NewGuid(), OwnerId = user.Id, Name = "Original" };
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var service = new BoardService(NullLogger<BoardService>.Instance, db);
        var response = await service.UpdateBoard(user, new UpdateBoardRequest(board.Id.ToString(), "Renamed"));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Name, Is.EqualTo("Renamed"));
        Assert.That((await db.Boards.FirstAsync(x => x.Id == board.Id)).Name, Is.EqualTo("Renamed"));
    }

    [Test]
    public async Task UpdateBoard_WhenBoardNotOwned_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var owner = CreateUser();
        var otherUser = CreateUser();
        var board = new Board { Id = Guid.NewGuid(), OwnerId = owner.Id, Name = "Original" };
        db.Boards.Add(board);
        await db.SaveChangesAsync();

        var service = new BoardService(NullLogger<BoardService>.Instance, db);
        var response = await service.UpdateBoard(otherUser, new UpdateBoardRequest(board.Id.ToString(), "Renamed"));

        Assert.That(response, Is.Null);
        Assert.That((await db.Boards.FirstAsync(x => x.Id == board.Id)).Name, Is.EqualTo("Original"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "a@b.com",
        UserName = "a@b.com",
        FirstName = "A",
        LastName = "B"
    };
}
