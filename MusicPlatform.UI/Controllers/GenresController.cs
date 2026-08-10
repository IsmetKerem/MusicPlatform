using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class GenresController : Controller
{
    private readonly IApiClient _api;

    public GenresController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<GenreListDto>>("/api/genres");
        return View(result.Data ?? new List<GenreListDto>());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var genresTask = _api.GetAsync<List<GenreListDto>>("/api/genres");
        var songsTask  = _api.GetAsync<List<SongListDto>>($"/api/genres/{id}/songs");

        await Task.WhenAll(genresTask, songsTask);

        var genre = genresTask.Result.Data?.FirstOrDefault(g => g.Id == id);

        if (genre is null)
        {
            TempData["Error"] = "Tür bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(new GenreDetailViewModel
        {
            Genre = genre,
            Songs = songsTask.Result.Data ?? new()
        });
    }
}