using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Playlist;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class PlaylistsController : Controller
{
    private readonly IApiClient _api;

    public PlaylistsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<PlaylistListDto>>("/api/playlists");
        return View(result.Data ?? new List<PlaylistListDto>());
    }

    [HttpGet("/Playlists/Mine")]
    public async Task<IActionResult> Mine()
    {
        var result = await _api.GetAsync<List<PlaylistListDto>>("/api/playlists");
        return Json(result);
    }

    [HttpPost("/Playlists/{playlistId:int}/AddSong/{songId:int}")]
    public async Task<IActionResult> AddSong(int playlistId, int songId)
    {
        var result = await _api.PostAsync<object>($"/api/playlists/{playlistId}/songs/{songId}");
        return Json(result);
    }
}