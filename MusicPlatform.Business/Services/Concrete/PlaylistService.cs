using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Playlist;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class PlaylistService : IPlaylistService
{
    private const int MaxPlaylistsPerUser = 50;

    private readonly AppDbContext _context;
    private readonly IPackageAuthorizationService _packageAuth;

    public PlaylistService(AppDbContext context, IPackageAuthorizationService packageAuth)
    {
        _context = context;
        _packageAuth = packageAuth;
    }

    public async Task<ApiResponse<List<PlaylistListDto>>> GetMyPlaylistsAsync(int userId)
    {
        var list = await _context.Playlists
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlaylistListDto
            {
                Id                   = p.Id,
                Name                 = p.Name,
                Description          = p.Description,
                CoverImageUrl        = p.CoverImageUrl,
                IsPublic             = p.IsPublic,
                SongCount            = p.PlaylistSongs.Count,
                TotalDurationSeconds = p.PlaylistSongs.Sum(ps => (int?)ps.Song.DurationInSeconds) ?? 0,
                CreatedAt            = p.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<PlaylistListDto>>.Ok(list);
    }

    public async Task<ApiResponse<PlaylistDetailDto>> GetByIdAsync(
        int playlistId, int userId, PackageLevel userPackage)
    {
        var playlist = await _context.Playlists
            .AsNoTracking()
            .Include(p => p.PlaylistSongs).ThenInclude(ps => ps.Song).ThenInclude(s => s.Artist)
            .Include(p => p.PlaylistSongs).ThenInclude(ps => ps.Song).ThenInclude(s => s.Album)
            .Include(p => p.PlaylistSongs).ThenInclude(ps => ps.Song)
                .ThenInclude(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist is null)
            return ApiResponse<PlaylistDetailDto>.Fail("Playlist bulunamadı.");

        if (playlist.UserId != userId && !playlist.IsPublic)
            return ApiResponse<PlaylistDetailDto>.Fail("Bu playlist'e erişim yetkiniz yok.");

        var dto = new PlaylistDetailDto
        {
            Id                   = playlist.Id,
            Name                 = playlist.Name,
            Description          = playlist.Description,
            CoverImageUrl        = playlist.CoverImageUrl,
            IsPublic             = playlist.IsPublic,
            CreatedAt            = playlist.CreatedAt,
            SongCount            = playlist.PlaylistSongs.Count,
            TotalDurationSeconds = playlist.PlaylistSongs.Sum(ps => ps.Song.DurationInSeconds),

            Songs = playlist.PlaylistSongs
                .OrderBy(ps => ps.SortOrder)
                .Select(ps => new SongListDto
                {
                    Id                  = ps.Song.Id,
                    Title               = ps.Song.Title,
                    ArtistId            = ps.Song.ArtistId,
                    ArtistName          = ps.Song.Artist.Name,
                    AlbumTitle          = ps.Song.Album?.Title,
                    CoverImageUrl       = ps.Song.CoverImageUrl,
                    DurationInSeconds   = ps.Song.DurationInSeconds,
                    DurationDisplay     = TimeSpan.FromSeconds(ps.Song.DurationInSeconds).ToString(@"m\:ss"),
                    PlayCount           = ps.Song.PlayCount,
                    RequiredPackage     = (int)ps.Song.RequiredPackage,
                    RequiredPackageName = ps.Song.RequiredPackage.ToString(),
                    CanPlay             = _packageAuth.CanAccess(userPackage, ps.Song.RequiredPackage),
                    Genres              = ps.Song.SongGenres.Select(sg => sg.Genre.Name).ToList()
                }).ToList()
        };

        return ApiResponse<PlaylistDetailDto>.Ok(dto);
    }

    public async Task<ApiResponse<PlaylistListDto>> CreateAsync(int userId, CreatePlaylistDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<PlaylistListDto>.Fail("Playlist adı boş olamaz.");

        var count = await _context.Playlists.CountAsync(p => p.UserId == userId);
        if (count >= MaxPlaylistsPerUser)
            return ApiResponse<PlaylistListDto>.Fail($"En fazla {MaxPlaylistsPerUser} playlist oluşturabilirsiniz.");

        var duplicate = await _context.Playlists
            .AnyAsync(p => p.UserId == userId && p.Name == dto.Name.Trim());
        if (duplicate)
            return ApiResponse<PlaylistListDto>.Fail("Bu adda bir playlist'iniz zaten var.");

        var playlist = new Playlist
        {
            UserId      = userId,
            Name        = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsPublic    = dto.IsPublic
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return ApiResponse<PlaylistListDto>.Ok(new PlaylistListDto
        {
            Id          = playlist.Id,
            Name        = playlist.Name,
            Description = playlist.Description,
            IsPublic    = playlist.IsPublic,
            SongCount   = 0,
            CreatedAt   = playlist.CreatedAt
        }, "Playlist oluşturuldu.");
    }

    public async Task<ApiResponse> UpdateAsync(int playlistId, int userId, CreatePlaylistDto dto)
    {
        var playlist = await FindOwnedAsync(playlistId, userId);
        if (playlist is null) return ApiResponse.Fail("Playlist bulunamadı veya yetkiniz yok.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse.Fail("Playlist adı boş olamaz.");

        playlist.Name        = dto.Name.Trim();
        playlist.Description = dto.Description?.Trim();
        playlist.IsPublic    = dto.IsPublic;

        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Playlist güncellendi.");
    }

    public async Task<ApiResponse> DeleteAsync(int playlistId, int userId)
    {
        var playlist = await FindOwnedAsync(playlistId, userId);
        if (playlist is null) return ApiResponse.Fail("Playlist bulunamadı veya yetkiniz yok.");

        _context.Playlists.Remove(playlist); 
        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Playlist silindi.");
    }

    public async Task<ApiResponse> AddSongAsync(int playlistId, int userId, int songId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist is null) return ApiResponse.Fail("Playlist bulunamadı veya yetkiniz yok.");

        if (!await _context.Songs.AnyAsync(s => s.Id == songId))
            return ApiResponse.Fail("Şarkı bulunamadı.");

        if (playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
            return ApiResponse.Fail("Bu şarkı playlist'te zaten var.");

        var nextOrder = playlist.PlaylistSongs.Count == 0
            ? 1
            : playlist.PlaylistSongs.Max(ps => ps.SortOrder) + 1;

        playlist.PlaylistSongs.Add(new PlaylistSong
        {
            SongId    = songId,
            SortOrder = nextOrder
        });

        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Şarkı playlist'e eklendi.");
    }

    public async Task<ApiResponse> RemoveSongAsync(int playlistId, int userId, int songId)
    {
        var playlist = await FindOwnedAsync(playlistId, userId);
        if (playlist is null) return ApiResponse.Fail("Playlist bulunamadı veya yetkiniz yok.");

        var link = await _context.PlaylistSongs
            .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

        if (link is null) return ApiResponse.Fail("Şarkı bu playlist'te değil.");

        _context.PlaylistSongs.Remove(link);
        await _context.SaveChangesAsync();
        return ApiResponse.Ok("Şarkı playlist'ten çıkarıldı.");
    }

    private Task<Playlist?> FindOwnedAsync(int playlistId, int userId) =>
        _context.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);
}