using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface IRecommendationService
{
    Task<ApiResponse<List<SongListDto>>> GetForUserAsync(int userId, PackageLevel userPackage, int count = 10);

    Task<ApiResponse<List<RecommendedSongDto>>> GetSimilarToSongAsync(
        int songId, PackageLevel userPackage, int count = 6, int? excludeUserId = null);

    Task<ApiResponse<List<RecommendedSongDto>>> GetPersonalizedAsync(
        int userId, PackageLevel userPackage, int count = 10);
    Task<bool> TrainModelAsync();
}