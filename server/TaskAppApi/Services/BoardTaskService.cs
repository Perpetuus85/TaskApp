using Microsoft.EntityFrameworkCore;
using TaskAppApi.Models;

namespace TaskAppApi.Services;

public class BoardTaskService(
    AppDbContext dbContext) : IBoardTaskService
{
    public async Task<BoardTask?> CreateTask(User user, CreateBoardTaskRequest request)
    {
        var boards = await dbContext.Boards.Where(x => x.OwnerId == user.Id).Select(y => y.Id).ToListAsync();
        var incBoardId = Guid.Parse(request.BoardId);
        if (!boards.Contains(incBoardId))
        {
            return null;
        }
        
        DateTime? dueAt = !string.IsNullOrEmpty(request.DueDate)
            ? DateTime.ParseExact(request.DueDate, "yyyy-MM-ddTHH:mm:ssZ", null)
            : null;
        
        var boardTask = new BoardTask
        {
            Summary = request.Summary,
            Description = request.Description,
            BoardId = incBoardId,
            CreatedByUserId = user.Id,
            DueAt = dueAt,
            Status = Enum.Parse<BoardTaskStatus>(request.Status, ignoreCase: true),
        };
        
        dbContext.BoardTasks.Add(boardTask);
        await dbContext.SaveChangesAsync();

        return boardTask;
    }

    public async Task<BoardTask?> UpdateTask(User user, UpdateBoardTaskRequest updateBoardTaskRequest)
    {
        var incGuid = Guid.Parse(updateBoardTaskRequest.Id);
        var boardTasks = await dbContext.BoardTasks
            .Where(x => x.CreatedByUserId == user.Id)
            .Select(y => y.Id)
            .ToListAsync();
        if (!boardTasks.Contains(incGuid))
        {
            return null;
        }
        
        var boardTask = dbContext.BoardTasks.FirstOrDefault(x => x.Id == incGuid);
        if (boardTask == null)
        {
            return null;
        }

        DateTime? dueAt = !string.IsNullOrEmpty(updateBoardTaskRequest.DueDate)
            ? DateTime.ParseExact(updateBoardTaskRequest.DueDate, "yyyy-MM-ddTHH:mm:ssZ", null)
            : null;
        
        boardTask.Summary = updateBoardTaskRequest.Summary;
        boardTask.Description = updateBoardTaskRequest.Description;
        boardTask.DueAt = dueAt;
        boardTask.Status = Enum.Parse<BoardTaskStatus>(updateBoardTaskRequest.Status, ignoreCase: true);
        boardTask.UpdatedAt = DateTime.UtcNow;
        boardTask.UpdatedByUserId = user.Id;
        await dbContext.SaveChangesAsync();
        
        return boardTask;
    }

    public async Task DeleteTask(User user, DeleteBoardTaskRequest deleteBoardTaskRequest)
    {
        var incGuid = Guid.Parse(deleteBoardTaskRequest.Id);
        var boardTasks = await dbContext.BoardTasks
            .Where(x => x.CreatedByUserId == user.Id)
            .Select(y => y.Id)
            .ToListAsync();
        if (!boardTasks.Contains(incGuid))
        {
            return;
        }
        
        var boardTask = dbContext.BoardTasks.FirstOrDefault(x => x.Id == incGuid);
        if (boardTask == null)
        {
            return;
        }
        dbContext.BoardTasks.Remove(boardTask);
        await dbContext.SaveChangesAsync();
    }

    public async Task<BoardTask?> GetTask(User user, Guid id)
    {
        var boardTasks = await dbContext.BoardTasks
            .Where(x => x.CreatedByUserId == user.Id)
            .Select(y => y.Id)
            .ToListAsync();
        if (!boardTasks.Contains(id))
        {
            return null;
        }
        
        var boardTask = dbContext.BoardTasks.FirstOrDefault(x => x.Id == id);

        return boardTask;
    }
}
