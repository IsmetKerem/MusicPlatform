using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService) => _favoriteService = favoriteService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _favoriteService.GetAllAsync(User.GetUserId(), User.GetPackageLevel()));

    [HttpGet("ids")]
    public async Task<IActionResult> GetIds()
        => Ok(await _favoriteService.GetFavoriteIdsAsync(User.GetUserId()));

    [HttpPost("toggle/{songId:int}")]
    public async Task<IActionResult> Toggle(int songId)
    {
        var result = await _favoriteService.ToggleAsync(User.GetUserId(), songId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}