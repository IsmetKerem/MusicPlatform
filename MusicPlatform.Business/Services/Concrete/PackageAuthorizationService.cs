using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Business.Services.Concrete;

public class PackageAuthorizationService : IPackageAuthorizationService
{
    private readonly AppDbContext _context;

    public PackageAuthorizationService(AppDbContext context) => _context = context;

    public bool CanAccess(PackageLevel userPackage, PackageLevel requiredPackage)
        => (int)userPackage >= (int)requiredPackage;

    public async Task<PackageCheckResult> CanUserPlaySongAsync(int userId, int songId)
    {
        var song = await _context.Songs
            .AsNoTracking()
            .Where(s => s.Id == songId)
            .Select(s => new { s.Id, s.Title, s.RequiredPackage })
            .FirstOrDefaultAsync();

        if (song is null)
            return new PackageCheckResult { SongExists = false, Allowed = false };

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.PackageLevel, u.PackageExpiresAt })
            .FirstOrDefaultAsync();

        if (user is null)
            return new PackageCheckResult { SongExists = true, Allowed = false, SongTitle = song.Title };

        var effectivePackage = user.PackageExpiresAt.HasValue && user.PackageExpiresAt.Value < DateTime.UtcNow
            ? PackageLevel.Basic
            : user.PackageLevel;

        return new PackageCheckResult
        {
            SongExists      = true,
            SongTitle       = song.Title,
            UserPackage     = effectivePackage,
            RequiredPackage = song.RequiredPackage,
            Allowed         = CanAccess(effectivePackage, song.RequiredPackage)
        };
    }
}