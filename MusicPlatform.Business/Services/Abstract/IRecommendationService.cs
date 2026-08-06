using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface IRecommendationService
{
    Task<ApiResponse<List<SongListDto>>> GetForUserAsync(int userId, PackageLevel userPackage, int count = 10);
    Task<ApiResponse<List<SongListDto>>> GetSimilarToSongAsync(int songId, PackageLevel userPackage, int count = 6);
}