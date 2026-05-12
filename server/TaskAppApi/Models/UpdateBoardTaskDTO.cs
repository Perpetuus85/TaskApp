namespace TaskAppApi.Models;

public record UpdateBoardTaskRequest(
    string Id,
    string Summary,
    string Description,
    string? DueDate,
    string Status);
