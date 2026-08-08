using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class SongsController : Controller
{
    private readonly IApiClient _api;

    public SongsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index(
        string? search = null,
        int? genreId = null,
        int? requiredPackage = null,
        bool onlyPlayable = false,
        int sortBy = 0,
        int page = 1)
    {
        var query = new List<string>
        {
            $"page={page}",
            "pageSize=24",
            $"sortBy={sortBy}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        if (genreId.HasValue)         query.Add($"genreId={genreId}");
        if (requiredPackage.HasValue) query.Add($"requiredPackage={requiredPackage}");
        if (onlyPlayable)             query.Add("onlyPlayable=true");

        var songsTask  = _api.GetAsync<PagedResult<SongListDto>>($"/api/songs?{string.Join("&", query)}");
        var genresTask = _api.GetAsync<List<GenreListDto>>("/api/genres");

        await Task.WhenAll(songsTask, genresTask);

        return View(new SongListViewModel
        {
            Result          = songsTask.Result.Data ?? new(),
            Genres          = genresTask.Result.Data ?? new(),
            Search          = search,
            GenreId         = genreId,
            RequiredPackage = requiredPackage,
            OnlyPlayable    = onlyPlayable,
            SortBy          = sortBy
        });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var songResult = await _api.GetAsync<SongListDto>($"/api/songs/{id}");

        if (!songResult.Success || songResult.Data is null)
        {
            TempData["Error"] = songResult.Message ?? "Şarkı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var song = songResult.Data;

        var recTask = _api.GetAsync<List<RecommendedSongDto>>(
            $"/api/recommendations/similar/{id}?count=8");

        var artistTask = _api.GetAsync<PagedResult<SongListDto>>(
            $"/api/songs?artistId={song.ArtistId}&pageSize=6");

        await Task.WhenAll(recTask, artistTask);

        return View(new SongDetailViewModel
        {
            Song            = song,
            Recommendations = recTask.Result.Data ?? new(),
            MoreFromArtist  = (artistTask.Result.Data?.Items ?? new())
                                  .Where(s => s.Id != id)
                                  .Take(5)
                                  .ToList()
        });
    }
}