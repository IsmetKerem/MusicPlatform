using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface ISongService
{
    Task<ApiResponse<PagedResult<SongListDto>>> GetAllAsync(SongFilterDto filter, PackageLevel userPackage);
    Task<ApiResponse<SongListDto>> GetByIdAsync(int songId, PackageLevel userPackage);
    Task<ApiResponse<List<SongListDto>>> GetPopularAsync(int count, PackageLevel userPackage);
    Task<ApiResponse<List<SongListDto>>> GetByGenreAsync(int genreId, PackageLevel userPackage);
}