using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class ArtistService : IArtistService
{
    private readonly AppDbContext _context;
    private readonly IPackageAuthorizationService _packageAuth;

    public ArtistService(AppDbContext context, IPackageAuthorizationService packageAuth)
    {
        _context = context;
        _packageAuth = packageAuth;
    }

    public async Task<ApiResponse<PagedResult<ArtistListDto>>> GetAllAsync(PageRequest page, string? search)
    {
        var query = _context.Artists.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => EF.Functions.Like(a.Name, $"%{search.Trim()}%"));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(a => new ArtistListDto
            {
                Id             = a.Id,
                Name           = a.Name,
                ImageUrl       = a.ImageUrl,
                Country        = a.Country,
                SongCount      = a.Songs.Count,
                TotalPlayCount = a.Songs.Sum(s => (long?)s.PlayCount) ?? 0
            })
            .ToListAsync();

        return ApiResponse<PagedResult<ArtistListDto>>.Ok(new PagedResult<ArtistListDto>
        {
            Items      = items,
            Page       = page.Page,
            PageSize   = page.PageSize,
            TotalCount = total
        });
    }

    public async Task<ApiResponse<ArtistDetailDto>> GetByIdAsync(int artistId, PackageLevel userPackage)
    {
        var artist = await _context.Artists
            .AsNoTracking()
            .Include(a => a.Albums)
                .ThenInclude(al => al.Songs)
            .Include(a => a.Songs).ThenInclude(s => s.Album)
            .Include(a => a.Songs).ThenInclude(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .FirstOrDefaultAsync(a => a.Id == artistId);

        if (artist is null)
            return ApiResponse<ArtistDetailDto>.Fail("Sanatçı bulunamadı.");

        var dto = new ArtistDetailDto
        {
            Id             = artist.Id,
            Name           = artist.Name,
            Bio            = artist.Bio,
            ImageUrl       = artist.ImageUrl,
            Country        = artist.Country,
            DebutYear      = artist.DebutYear,
            SongCount      = artist.Songs.Count,
            TotalPlayCount = artist.Songs.Sum(s => s.PlayCount),

            TopGenres = artist.Songs
                .SelectMany(s => s.SongGenres)
                .GroupBy(sg => sg.Genre.Name)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList(),

            Albums = artist.Albums
                .OrderByDescending(al => al.ReleaseDate)
                .Select(al => new AlbumBriefDto
                {
                    Id            = al.Id,
                    Title         = al.Title,
                    CoverImageUrl = al.CoverImageUrl,
                    ReleaseDate   = al.ReleaseDate,
                    SongCount     = al.Songs.Count
                }).ToList(),

            Songs = artist.Songs
                .OrderByDescending(s => s.PlayCount)
                .Select(s => new SongListDto
                {
                    Id                  = s.Id,
                    Title               = s.Title,
                    ArtistId            = artist.Id,
                    ArtistName          = artist.Name,
                    AlbumTitle          = s.Album?.Title,
                    CoverImageUrl       = s.CoverImageUrl,
                    DurationInSeconds   = s.DurationInSeconds,
                    DurationDisplay     = TimeSpan.FromSeconds(s.DurationInSeconds).ToString(@"m\:ss"),
                    PlayCount           = s.PlayCount,
                    RequiredPackage     = (int)s.RequiredPackage,
                    RequiredPackageName = s.RequiredPackage.ToString(),
                    CanPlay             = _packageAuth.CanAccess(userPackage, s.RequiredPackage),
                    Genres              = s.SongGenres.Select(sg => sg.Genre.Name).ToList()
                }).ToList()
        };

        return ApiResponse<ArtistDetailDto>.Ok(dto);
    }
}