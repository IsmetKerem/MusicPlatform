using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Song;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class FavoritesController : Controller
{
    private readonly IApiClient _api;

    public FavoritesController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<SongListDto>>("/api/favorites");
        return View(result.Data ?? new List<SongListDto>());
    }

    [HttpGet("/Favorites/Ids")]
    public async Task<IActionResult> Ids()
    {
        var result = await _api.GetAsync<List<int>>("/api/favorites/ids");
        return Json(result);
    }

    [HttpPost("/Favorites/Toggle/{songId:int}")]
    public async Task<IActionResult> Toggle(int songId)
    {
        var result = await _api.PostAsync<bool>($"/api/favorites/toggle/{songId}");
        return Json(result);
    }
}