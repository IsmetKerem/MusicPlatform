using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface IFavoriteService
{
    Task<ApiResponse<List<SongListDto>>> GetAllAsync(int userId, PackageLevel userPackage);
    Task<ApiResponse<bool>> ToggleAsync(int userId, int songId);
    Task<ApiResponse<List<int>>> GetFavoriteIdsAsync(int userId);
}