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

    public async Task<ApiResponse<PagedResult<SongListDto>>> GetAllAsync(
        SongFilterDto filter, PackageLevel userPackage)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(s =>
                EF.Functions.Like(s.Title, $"%{term}%") ||
                EF.Functions.Like(s.Artist.Name, $"%{term}%"));
        }

        if (filter.GenreId.HasValue)
            query = query.Where(s => s.SongGenres.Any(sg => sg.GenreId == filter.GenreId.Value));

        if (filter.ArtistId.HasValue)
            query = query.Where(s => s.ArtistId == filter.ArtistId.Value);

        if (filter.AlbumId.HasValue)
            query = query.Where(s => s.AlbumId == filter.AlbumId.Value);

        if (filter.RequiredPackage.HasValue)
            query = query.Where(s => (int)s.RequiredPackage == filter.RequiredPackage.Value);

        if (filter.OnlyPlayable)
            query = query.Where(s => (int)s.RequiredPackage <= (int)userPackage);

        query = filter.SortBy switch
        {
            SongSortBy.ArtistName => query.OrderBy(s => s.Artist.Name).ThenBy(s => s.Title),
            SongSortBy.MostPlayed => query.OrderByDescending(s => s.PlayCount),
            SongSortBy.Newest     => query.OrderByDescending(s => s.CreatedAt),
            SongSortBy.Duration   => query.OrderBy(s => s.DurationInSeconds),
            _                     => query.OrderBy(s => s.Title)
        };

        var total = await query.CountAsync();

        var songs = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<SongListDto>>.Ok(new PagedResult<SongListDto>
        {
            Items      = songs.Select(s => Map(s, userPackage)).ToList(),
            Page       = filter.Page,
            PageSize   = filter.PageSize,
            TotalCount = total
        });
    }

    public async Task<ApiResponse<SongListDto>> GetByIdAsync(int songId, PackageLevel userPackage)
    {
        var song = await BaseQuery().FirstOrDefaultAsync(s => s.Id == songId);

        return song is null
            ? ApiResponse<SongListDto>.Fail("Şarkı bulunamadı.")
            : ApiResponse<SongListDto>.Ok(Map(song, userPackage));
    }

    public async Task<ApiResponse<List<SongListDto>>> GetPopularAsync(int count, PackageLevel userPackage)
    {
        if (count is < 1 or > 50) count = 10;

        var songs = await BaseQuery()
            .OrderByDescending(s => s.PlayCount)
            .Take(count)
            .ToListAsync();

        return ApiResponse<List<SongListDto>>.Ok(songs.Select(s => Map(s, userPackage)).ToList());
    }

    public async Task<ApiResponse<List<SongListDto>>> GetByGenreAsync(int genreId, PackageLevel userPackage)
    {
        var exists = await _context.Genres.AnyAsync(g => g.Id == genreId);
        if (!exists) return ApiResponse<List<SongListDto>>.Fail("Tür bulunamadı.");

        var songs = await BaseQuery()
            .Where(s => s.SongGenres.Any(sg => sg.GenreId == genreId))
            .OrderBy(s => s.Artist.Name).ThenBy(s => s.Title)
            .ToListAsync();

        return ApiResponse<List<SongListDto>>.Ok(songs.Select(s => Map(s, userPackage)).ToList());
    }

    private IQueryable<Entity.Concrete.Song> BaseQuery() =>
        _context.Songs
            .AsNoTracking()
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre);

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