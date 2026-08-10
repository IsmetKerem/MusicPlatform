using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

public class HomeController : Controller
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokenStore;

    public HomeController(IApiClient api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    [RequireLogin]
    public async Task<IActionResult> Index()
    {
        var popularTask = _api.GetAsync<List<SongListDto>>("/api/songs/popular?count=12");
        var recTask     = _api.GetAsync<List<RecommendedSongDto>>("/api/recommendations/for-me?count=12");
        var genreTask   = _api.GetAsync<List<GenreListDto>>("/api/genres");
        var newestTask  = _api.GetAsync<PagedResult<SongListDto>>("/api/songs?sortBy=3&pageSize=12");

        await Task.WhenAll(popularTask, recTask, genreTask, newestTask);
        var allSongsTask = _api.GetAsync<PagedResult<SongListDto>>("/api/songs?pageSize=200");
        await allSongsTask;

        var all = allSongsTask.Result.Data?.Items ?? new();

        var model = new HomeViewModel
        {
            Popular         = popularTask.Result.Data ?? new(),
            Recommendations = recTask.Result.Data ?? new(),
            Genres          = genreTask.Result.Data ?? new(),
            Newest          = newestTask.Result.Data?.Items ?? new(),
            TotalSongs      = all.Count,
            UnlockedSongs   = all.Count(s => s.CanPlay),
            TierCounts      = all.GroupBy(s => s.RequiredPackage)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        

        return View(model);
    }

    [Route("/Home/Error/{code:int?}")]
    public IActionResult Error(int? code)
    {
        ViewData["StatusCode"] = code ?? 500;
        return View();
    }
}