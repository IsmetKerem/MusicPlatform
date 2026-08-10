using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class ArtistsController : Controller
{
    private readonly IApiClient _api;

    public ArtistsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search = null, int page = 1)
    {
        var query = $"page={page}&pageSize=24";

        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var result = await _api.GetAsync<PagedResult<ArtistListDto>>($"/api/artists?{query}");

        return View(new ArtistListViewModel
        {
            Result = result.Data ?? new(),
            Search = search
        });
    }


    public async Task<IActionResult> Detail(int id)
    {
        var result = await _api.GetAsync<ArtistDetailDto>($"/api/artists/{id}");

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Message ?? "Sanatçı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}