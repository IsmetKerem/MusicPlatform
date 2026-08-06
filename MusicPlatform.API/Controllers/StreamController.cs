using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamController : ControllerBase
{
    private readonly IPackageAuthorizationService _packageAuth;
    private readonly IStreamService _streamService;
    private readonly AppDbContext _context;
    private readonly INotificationService _notification;

    public StreamController(
        IPackageAuthorizationService packageAuth,
        IStreamService streamService,
        AppDbContext context,
        INotificationService notification)
    {
        _packageAuth = packageAuth;
        _streamService = streamService;
        _context = context;
        _notification = notification;
    }


    [HttpGet("check/{songId:int}")]
    public async Task<IActionResult> Check(int songId)
    {
        var userId = User.GetUserId();
        var check = await _packageAuth.CanUserPlaySongAsync(userId, songId);

        if (!check.SongExists)
            return NotFound(ApiResponse.Fail("Şarkı bulunamadı."));

        if (!check.Allowed)
        {

            _ = _notification.SendUpgradeInvitationAsync(userId, songId);

            return StatusCode(403, new ApiResponse<PackageDeniedDto>
            {
                Success = false,
                Message = "Mevcut paketiniz bu şarkıyı desteklememektedir. Lütfen paketinizi yükseltin.",
                Data = new PackageDeniedDto
                {
                    SongId              = songId,
                    SongTitle           = check.SongTitle,
                    RequiredPackage     = (int)check.RequiredPackage,
                    RequiredPackageName = check.RequiredPackage.ToString(),
                    UserPackage         = (int)check.UserPackage,
                    UserPackageName     = check.UserPackage.ToString()
                }
            });
        }

        var song = await _context.Songs
            .AsNoTracking()
            .Include(s => s.Artist)
            .FirstAsync(s => s.Id == songId);

        return Ok(ApiResponse<PlaybackResultDto>.Ok(new PlaybackResultDto
        {
            SongId            = song.Id,
            Title             = song.Title,
            ArtistName        = song.Artist.Name,
            StreamUrl         = $"/api/stream/{song.Id}",
            DurationInSeconds = song.DurationInSeconds,
            CoverImageUrl     = song.CoverImageUrl
        }));
    }


    [HttpGet("{songId:int}")]
    public async Task<IActionResult> Play(int songId)
    {
        var userId = User.GetUserId();
        var check = await _packageAuth.CanUserPlaySongAsync(userId, songId);

        if (!check.SongExists)
            return NotFound(ApiResponse.Fail("Şarkı bulunamadı."));

        if (!check.Allowed)
            return StatusCode(403, ApiResponse.Fail(
                "Mevcut paketiniz bu şarkıyı desteklememektedir. Lütfen paketinizi yükseltin."));

        var fileName = await _context.Songs
            .Where(s => s.Id == songId)
            .Select(s => s.FileName)
            .FirstAsync();

        var physicalPath = _streamService.ResolvePhysicalPath(fileName);

        if (physicalPath is null)
            return NotFound(ApiResponse.Fail("Ses dosyası sunucuda bulunamadı."));

        var stream = new FileStream(
            physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 64 * 1024, useAsync: true);

        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }

    [HttpPost("log/{songId:int}")]
    public async Task<IActionResult> LogListening(int songId, [FromQuery] int seconds = 0)
    {
        var userId = User.GetUserId();
        var check = await _packageAuth.CanUserPlaySongAsync(userId, songId);

        if (!check.Allowed)
            return StatusCode(403, ApiResponse.Fail("Bu şarkı için yetkiniz yok."));

        await _streamService.LogListeningAsync(userId, songId, seconds);
        return Ok(ApiResponse.Ok("Dinleme kaydedildi."));
    }
}