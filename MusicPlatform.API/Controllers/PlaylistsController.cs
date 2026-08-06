using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Shared.DTOs.Playlist;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(IPlaylistService playlistService) => _playlistService = playlistService;

    [HttpGet]
    public async Task<IActionResult> GetMine()
        => Ok(await _playlistService.GetMyPlaylistsAsync(User.GetUserId()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _playlistService.GetByIdAsync(id, User.GetUserId(), User.GetPackageLevel());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistDto dto)
    {
        var result = await _playlistService.CreateAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePlaylistDto dto)
    {
        var result = await _playlistService.UpdateAsync(id, User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _playlistService.DeleteAsync(id, User.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/songs/{songId:int}")]
    public async Task<IActionResult> AddSong(int id, int songId)
    {
        var result = await _playlistService.AddSongAsync(id, User.GetUserId(), songId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/songs/{songId:int}")]
    public async Task<IActionResult> RemoveSong(int id, int songId)
    {
        var result = await _playlistService.RemoveSongAsync(id, User.GetUserId(), songId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}