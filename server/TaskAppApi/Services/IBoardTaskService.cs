using TaskAppApi.Models;

namespace TaskAppApi.Services;

public interface IBoardTaskService
{
    Task<BoardTask?> CreateTask(User user, CreateBoardTaskRequest createBoardTaskRequest);
    Task<BoardTask?> UpdateTask(User user, UpdateBoardTaskRequest updateBoardTaskRequest);
    Task DeleteTask(User user, DeleteBoardTaskRequest deleteBoardTaskRequest);
    Task<BoardTask?> GetTask(User user, Guid id);
}