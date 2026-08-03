using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class SongService : ISongService
{
    private readonly AppDbContext _context;
    private readonly IPackageAuthorizationService _packageAuth;

    public SongService(AppDbContext context, IPackageAuthorizationService packageAuth)
    {
        _context = context;
        _packageAuth = packageAuth;
    }

    public async Task<ApiResponse<List<SongListDto>>> GetAllAsync(PackageLevel userPackage)
    {
        var songs = await _context.Songs
            .AsNoTracking()
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .OrderBy(s => s.Artist.Name).ThenBy(s => s.Title)
            .ToListAsync();

        var list = songs.Select(s => Map(s, userPackage)).ToList();
        return ApiResponse<List<SongListDto>>.Ok(list);
    }

    public async Task<ApiResponse<SongListDto>> GetByIdAsync(int songId, PackageLevel userPackage)
    {
        var song = await _context.Songs
            .AsNoTracking()
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .FirstOrDefaultAsync(s => s.Id == songId);

        return song is null
            ? ApiResponse<SongListDto>.Fail("Şarkı bulunamadı.")
            : ApiResponse<SongListDto>.Ok(Map(song, userPackage));
    }

    private SongListDto Map(Entity.Concrete.Song s, PackageLevel userPackage) => new()
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
    };
}