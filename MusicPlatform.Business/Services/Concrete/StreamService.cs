using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.Business.Services.Concrete;

public class StreamService : IStreamService
{
    private readonly AppDbContext _context;
    private readonly string _musicFolder;

    public StreamService(AppDbContext context, IConfiguration config, IHostEnvironment env)
    {
        _context = context;
        _musicFolder = Path.Combine(
            env.ContentRootPath,
            config["MusicSettings:MusicFolder"] ?? "App_Data/Music");
    }

    public string? ResolvePhysicalPath(string fileName)
    {

        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName)) return null;

        var fullPath = Path.Combine(_musicFolder, safeName);

        var normalizedFolder = Path.GetFullPath(_musicFolder);
        var normalizedFile   = Path.GetFullPath(fullPath);

        if (!normalizedFile.StartsWith(normalizedFolder, StringComparison.Ordinal))
            return null;

        return File.Exists(normalizedFile) ? normalizedFile : null;
    }

    public async Task LogListeningAsync(int userId, int songId, int listenedSeconds)
    {
        _context.ListeningHistories.Add(new ListeningHistory
        {
            UserId          = userId,
            SongId          = songId,
            ListenedAt      = DateTime.UtcNow,
            ListenedSeconds = listenedSeconds,
            IsCompleted     = false
        });

        await _context.Songs
            .Where(s => s.Id == songId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.PlayCount, x => x.PlayCount + 1));

        await _context.SaveChangesAsync();
    }
}