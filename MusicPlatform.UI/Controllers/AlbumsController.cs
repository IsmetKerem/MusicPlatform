using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.Shared.DTOs.Song;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class AlbumsController : Controller
{
    private readonly IApiClient _api;

    public AlbumsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<AlbumBriefDto>>("/api/albums");
        return View(result.Data ?? new List<AlbumBriefDto>());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var result = await _api.GetAsync<List<SongListDto>>($"/api/albums/{id}/songs");

        if (!result.Success)
        {
            TempData["Error"] = result.Message ?? "Albüm bulunamadı.";
            return RedirectToAction("Index", "Artists");
        }

        var songs = result.Data ?? new List<SongListDto>();
        ViewData["AlbumTitle"] = songs.FirstOrDefault()?.AlbumTitle ?? "Albüm";

        return View(songs);
    }
}