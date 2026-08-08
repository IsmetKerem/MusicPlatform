using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Options;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class StreamController : Controller
{
    private readonly IApiClient _api;
    private readonly ApiSettings _settings;

    public StreamController(IApiClient api, IOptions<ApiSettings> settings)
    {
        _api = api;
        _settings = settings.Value;
    }

    [HttpGet("/Stream/Play/{songId:int}")]
    public async Task<IActionResult> Play(int songId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/stream/{songId}");

        if (Request.Headers.TryGetValue("Range", out var range))
            request.Headers.TryAddWithoutValidation("Range", range.ToString());

        var response = await _api.SendRawAsync(request);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            response.Dispose();
            return StatusCode(403);
        }

        if (!response.IsSuccessStatusCode)
        {
            var code = (int)response.StatusCode;
            response.Dispose();
            return StatusCode(code);
        }

        if (response.Content.Headers.ContentRange is not null)
            Response.Headers["Content-Range"] = response.Content.Headers.ContentRange.ToString();

        if (response.Headers.AcceptRanges.Count > 0)
            Response.Headers["Accept-Ranges"] = string.Join(",", response.Headers.AcceptRanges);
        else
            Response.Headers["Accept-Ranges"] = "bytes";

        Response.StatusCode = (int)response.StatusCode;

        var stream = await response.Content.ReadAsStreamAsync();

        return new FileStreamResult(stream, response.Content.Headers.ContentType?.ToString() ?? "audio/mpeg")
        {
            EnableRangeProcessing = false 
        };
    }

    [HttpGet("/Stream/Check/{songId:int}")]
    public async Task<IActionResult> Check(int songId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/stream/check/{songId}");
        var response = await _api.SendRawAsync(request);

        var body = await response.Content.ReadAsStringAsync();
        response.Dispose();

        return Content(body, "application/json");
    }

    [HttpPost("/Stream/Log/{songId:int}")]
    public async Task<IActionResult> Log(int songId, [FromQuery] int seconds)
    {
        await _api.PostAsync<object>($"/api/stream/log/{songId}?seconds={seconds}");
        return Ok();
    }
}