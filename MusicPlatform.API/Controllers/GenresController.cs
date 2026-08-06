using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;
    private readonly ISongService _songService;

    public GenresController(IGenreService genreService, ISongService songService)
    {
        _genreService = genreService;
        _songService = songService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _genreService.GetAllAsync());

    [HttpGet("{id:int}/songs")]
    public async Task<IActionResult> GetSongs(int id)
    {
        var result = await _songService.GetByGenreAsync(id, User.GetPackageLevel());
        return result.Success ? Ok(result) : NotFound(result);
    }
}