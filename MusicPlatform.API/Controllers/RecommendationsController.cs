using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
        => _recommendationService = recommendationService;

    [HttpGet("for-me")]
    public async Task<IActionResult> ForMe([FromQuery] int count = 10)
        => Ok(await _recommendationService.GetPersonalizedAsync(
            User.GetUserId(), User.GetPackageLevel(), count));


    [HttpGet("similar/{songId:int}")]
    public async Task<IActionResult> Similar(int songId, [FromQuery] int count = 6)
    {
        var result = await _recommendationService.GetSimilarToSongAsync(
            songId, User.GetPackageLevel(), count, User.GetUserId());

        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpPost("train")]
    public async Task<IActionResult> Train()
    {
        var trained = await _recommendationService.TrainModelAsync();
        return Ok(new
        {
            success = true,
            trained,
            message = trained
                ? "ML.NET modeli başarıyla eğitildi."
                : "Yetersiz veri — sistem co-occurrence algoritmasıyla çalışıyor."
        });
    }
}