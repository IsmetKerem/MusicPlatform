using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Playlist;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
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

        return View(new PlaylistPageViewModel
        {
            Playlists = result.Data ?? new()
        });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var result = await _api.GetAsync<PlaylistDetailDto>($"/api/playlists/{id}");

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Message ?? "Playlist bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePlaylistViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Playlist adı gerekli.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.PostAsync<PlaylistListDto>("/api/playlists", new CreatePlaylistDto
        {
            Name        = model.Name,
            Description = model.Description,
            IsPublic    = model.IsPublic
        });

        TempData[result.Success ? "Success" : "Error"] =
            result.Message ?? (result.Success ? "Playlist oluşturuldu." : "Oluşturulamadı.");

        return result.Success && result.Data is not null
            ? RedirectToAction(nameof(Detail), new { id = result.Data.Id })
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreatePlaylistViewModel model)
    {
        if (!ModelState.IsValid || model.Id is null)
        {
            TempData["Error"] = "Playlist adı gerekli.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.PutAsync<object>($"/api/playlists/{model.Id}", new CreatePlaylistDto
        {
            Name        = model.Name,
            Description = model.Description,
            IsPublic    = model.IsPublic
        });

        TempData[result.Success ? "Success" : "Error"] = result.Message ?? "İşlem tamamlandı.";
        return RedirectToAction(nameof(Detail), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAsync<object>($"/api/playlists/{id}");

        TempData[result.Success ? "Success" : "Error"] = result.Message ?? "İşlem tamamlandı.";
        return RedirectToAction(nameof(Index));
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

    [HttpPost("/Playlists/{playlistId:int}/RemoveSong/{songId:int}")]
    public async Task<IActionResult> RemoveSong(int playlistId, int songId)
    {
        var result = await _api.DeleteAsync<object>($"/api/playlists/{playlistId}/songs/{songId}");
        return Json(result);
    }
}