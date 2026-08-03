using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Abstract;

public interface ISongService
{
    Task<ApiResponse<List<SongListDto>>> GetAllAsync(PackageLevel userPackage);
    Task<ApiResponse<SongListDto>> GetByIdAsync(int songId, PackageLevel userPackage);
}