using Microsoft.EntityFrameworkCore;
using TaskAppApi.Models;

namespace TaskAppApi.Services;

public class BoardService(
    ILogger<BoardService> logger,
    AppDbContext dbContext) : IBoardService
{
    public async Task<List<Board>> GetAllBoardsForUser(Guid userId)
    {
        return await dbContext.Boards.Where(b => b.OwnerId == userId).ToListAsync();
    }

    public async Task<CreateBoardResponse> CreateBoard(User user, CreateBoardRequest request)
    {
        var board = new Board
        {
            OwnerId = user.Id,
            Name = request.Name
        };
        dbContext.Boards.Add(board);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to create a new board");
            return new CreateBoardResponse(false, e.Message);
        }
        
        return new CreateBoardResponse();
    }

    public async Task<Board?> GetBoardWithTasks(User user, Guid id)
    {
        return await dbContext.Boards
            .Include(x => x.BoardTasks)
            .FirstOrDefaultAsync(x => x.OwnerId == user.Id && x.Id == id);
    }

    public async Task<Board?> UpdateBoard(User user, UpdateBoardRequest request)
    {
        var board = await dbContext.Boards
            .Include(x => x.BoardTasks)
            .FirstOrDefaultAsync(x => x.OwnerId == user.Id && x.Id == Guid.Parse(request.Id));
        if (board == null)
            return null;
        
        board.Name = request.Name;
        await dbContext.SaveChangesAsync();
        return board;
    }
}