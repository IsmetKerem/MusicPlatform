using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumService _albumService;

    public AlbumsController(IAlbumService albumService) => _albumService = albumService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _albumService.GetAllAsync());

    [HttpGet("{id:int}/songs")]
    public async Task<IActionResult> GetSongs(int id)
    {
        var result = await _albumService.GetSongsAsync(id, User.GetPackageLevel());
        return result.Success ? Ok(result) : NotFound(result);
    }
}