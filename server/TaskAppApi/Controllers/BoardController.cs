using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskAppApi.Models;
using TaskAppApi.Services;

namespace TaskAppApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardController(
    ILogger<BoardController> logger,
    IBoardService boardService,
    UserManager<User> userManager) : ControllerBase
{
    [HttpGet("GetAll")]
    [Authorize]
    public async Task<IResult> GetAllBoardsForUser()
    {
        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Getting all boards for user with Id {Id}", user!.Id);
        var boards = await boardService.GetAllBoardsForUser(user.Id);
        return Results.Ok(boards);
    }

    [HttpPost("Create")]
    [Authorize]
    public async Task<IResult> CreateBoard(CreateBoardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Board name is required.");
        }

        var user = await userManager.GetUserAsync(User);
        var response = await boardService.CreateBoard(user!, request);
        return response.Success ? Results.Ok() : Results.BadRequest(response.ErrorMessage);
    }

    [HttpGet("GetBoardWithTasksById/{id}")]
    [Authorize]
    public async Task<IResult> GetBoardWithTasks(Guid id)
    {
        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Getting boards with Id {BoardId} for user with Id {Id}", id, user!.Id);
        var response = await boardService.GetBoardWithTasks(user, id);
        return response != null ? Results.Ok(response) : Results.BadRequest();
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<IResult> UpdateBoard(UpdateBoardRequest request)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            return Results.BadRequest("Board ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Board name is required.");
        }

        var user = await userManager.GetUserAsync(User);
        logger.LogInformation("Updating boards with Id {BoardId} for user with Id {Id}", request.Id, user!.Id);
        var response = await boardService.UpdateBoard(user, request);
        return response != null ? Results.Ok(response) : Results.BadRequest();
    }
}
