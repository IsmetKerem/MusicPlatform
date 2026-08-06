using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;


public class RecommendationService : IRecommendationService
{
    private readonly ISongService _songService;

    public RecommendationService(ISongService songService) => _songService = songService;

    public Task<ApiResponse<List<SongListDto>>> GetForUserAsync(
        int userId, PackageLevel userPackage, int count = 10)
        => _songService.GetPopularAsync(count, userPackage);

    public Task<ApiResponse<List<SongListDto>>> GetSimilarToSongAsync(
        int songId, PackageLevel userPackage, int count = 6)
        => _songService.GetPopularAsync(count, userPackage);
}