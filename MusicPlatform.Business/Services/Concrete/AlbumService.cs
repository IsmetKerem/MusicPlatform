using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class AlbumService : IAlbumService
{
    private readonly AppDbContext _context;
    private readonly ISongService _songService;

    public AlbumService(AppDbContext context, ISongService songService)
    {
        _context = context;
        _songService = songService;
    }

    public async Task<ApiResponse<List<AlbumBriefDto>>> GetAllAsync()
    {
        var albums = await _context.Albums
            .AsNoTracking()
            .OrderBy(a => a.Artist.Name).ThenBy(a => a.Title)
            .Select(a => new AlbumBriefDto
            {
                Id            = a.Id,
                Title         = a.Title,
                CoverImageUrl = a.CoverImageUrl,
                ReleaseDate   = a.ReleaseDate,
                SongCount     = a.Songs.Count
            })
            .ToListAsync();

        return ApiResponse<List<AlbumBriefDto>>.Ok(albums);
    }

    public async Task<ApiResponse<List<SongListDto>>> GetSongsAsync(int albumId, PackageLevel userPackage)
    {
        var exists = await _context.Albums.AnyAsync(a => a.Id == albumId);
        if (!exists) return ApiResponse<List<SongListDto>>.Fail("Albüm bulunamadı.");

        var result = await _songService.GetAllAsync(
            new SongFilterDto { AlbumId = albumId, PageSize = 50 }, userPackage);

        return ApiResponse<List<SongListDto>>.Ok(result.Data!.Items);
    }
}