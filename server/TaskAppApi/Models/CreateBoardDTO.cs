namespace TaskAppApi.Models;

public record CreateBoardRequest(string Name);
public record CreateBoardResponse(bool Success = true, string ErrorMessage = "");