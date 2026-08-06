using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface IAlbumService
{
    Task<ApiResponse<List<AlbumBriefDto>>> GetAllAsync();
    Task<ApiResponse<List<SongListDto>>> GetSongsAsync(int albumId, PackageLevel userPackage);
}