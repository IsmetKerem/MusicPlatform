using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;
    private readonly IPackageAuthorizationService _packageAuth;

    public FavoriteService(AppDbContext context, IPackageAuthorizationService packageAuth)
    {
        _context = context;
        _packageAuth = packageAuth;
    }

    public async Task<ApiResponse<List<SongListDto>>> GetAllAsync(int userId, PackageLevel userPackage)
    {
        var songs = await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => f.Song)
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .ToListAsync();

        var list = songs.Select(s => new SongListDto
        {
            Id                  = s.Id,
            Title               = s.Title,
            ArtistId            = s.ArtistId,
            ArtistName          = s.Artist.Name,
            AlbumTitle          = s.Album?.Title,
            CoverImageUrl       = s.CoverImageUrl,
            DurationInSeconds   = s.DurationInSeconds,
            DurationDisplay     = TimeSpan.FromSeconds(s.DurationInSeconds).ToString(@"m\:ss"),
            PlayCount           = s.PlayCount,
            RequiredPackage     = (int)s.RequiredPackage,
            RequiredPackageName = s.RequiredPackage.ToString(),
            CanPlay             = _packageAuth.CanAccess(userPackage, s.RequiredPackage),
            Genres              = s.SongGenres.Select(sg => sg.Genre.Name).ToList()
        }).ToList();

        return ApiResponse<List<SongListDto>>.Ok(list);
    }

    public async Task<ApiResponse<bool>> ToggleAsync(int userId, int songId)
    {
        var songExists = await _context.Songs.AnyAsync(s => s.Id == songId);
        if (!songExists) return ApiResponse<bool>.Fail("Şarkı bulunamadı.");

        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

        if (existing is not null)
        {
            _context.Favorites.Remove(existing);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(false, "Favorilerden çıkarıldı.");
        }

        _context.Favorites.Add(new Favorite { UserId = userId, SongId = songId });
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Favorilere eklendi.");
    }

    public async Task<ApiResponse<List<int>>> GetFavoriteIdsAsync(int userId)
    {
        var ids = await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.SongId)
            .ToListAsync();

        return ApiResponse<List<int>>.Ok(ids);
    }
}