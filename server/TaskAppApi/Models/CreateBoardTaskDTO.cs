namespace TaskAppApi.Models;

public record CreateBoardTaskRequest(string Summary, string Description, string? DueDate, string Status, string BoardId);