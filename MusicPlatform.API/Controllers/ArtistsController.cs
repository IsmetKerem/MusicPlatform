using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Shared.Common;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _artistService;

    public ArtistsController(IArtistService artistService) => _artistService = artistService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PageRequest page, [FromQuery] string? search)
        => Ok(await _artistService.GetAllAsync(page, search));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _artistService.GetByIdAsync(id, User.GetPackageLevel());
        return result.Success ? Ok(result) : NotFound(result);
    }
}