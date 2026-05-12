using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardTaskController(ILogger<BoardTaskController> logger,
    IBoardTaskService boardTaskService,
    UserManager<User> userManager) : ControllerBase
{
    [HttpPost("Create")]
    [Authorize]
    public async Task<IResult> CreateTask([FromBody] CreateBoardTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Summary) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            !Guid.TryParse(request.BoardId, out var boardId) ||
            boardId == Guid.Empty)
        {
            return Results.BadRequest("Valid board ID, summary, and description are required.");
        }

        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Creating task for user with id {Id} with payload {Payload}", user!.Id, request.ToString());
        var response = await boardTaskService.CreateTask(user, request);
        return Results.Ok(response);
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<IResult> UpdateTask([FromBody] UpdateBoardTaskRequest request)
    {
        if (!Guid.TryParse(request.Id, out var taskId) ||
            taskId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Summary) ||
            string.IsNullOrWhiteSpace(request.Description))
        {
            return Results.BadRequest("Valid task ID, summary, and description are required.");
        }

        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Updating task for user with id {Id} with payload {Payload}", user!.Id, request.ToString());
        var response = await boardTaskService.UpdateTask(user, request);
        return Results.Ok(response);
    }
    
    [HttpPost("Delete")]
    [Authorize]
    public async Task<IResult> DeleteTask([FromBody] DeleteBoardTaskRequest request)
    {
        if (!Guid.TryParse(request.Id, out var taskId) || taskId == Guid.Empty)
        {
            return Results.BadRequest("Valid task ID is required.");
        }

        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Deleting task for user with id {Id}", user!.Id);
        await boardTaskService.DeleteTask(user, request);
        return Results.Ok();
    }
    
    [HttpGet("GetById/{id}")]
    [Authorize]
    public async Task<IResult> GetTask(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Results.BadRequest("Valid task ID is required.");
        }

        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Getting task for user with id {Id}", user!.Id);
        var response = await boardTaskService.GetTask(user, id);
        return Results.Ok(response);
    }
}
