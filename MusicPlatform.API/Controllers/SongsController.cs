using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController : ControllerBase
{
    private readonly ISongService _songService;

    public SongsController(ISongService songService) => _songService = songService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SongFilterDto filter)
        => Ok(await _songService.GetAllAsync(filter, User.GetPackageLevel()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _songService.GetByIdAsync(id, User.GetPackageLevel());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular([FromQuery] int count = 10)
        => Ok(await _songService.GetPopularAsync(count, User.GetPackageLevel()));
}