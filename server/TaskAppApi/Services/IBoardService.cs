using TaskAppApi.Models;

namespace TaskAppApi.Services;

public interface IBoardService
{
    Task<List<Board>> GetAllBoardsForUser(Guid userId);
    Task<CreateBoardResponse> CreateBoard(User user, CreateBoardRequest request);
    Task<Board?> GetBoardWithTasks(User user, Guid id);
    Task<Board?> UpdateBoard(User user, UpdateBoardRequest request);
}