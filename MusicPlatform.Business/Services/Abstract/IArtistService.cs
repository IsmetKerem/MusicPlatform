using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;

namespace MusicPlatform.Business.Services.Abstract;

public interface IArtistService
{
    Task<ApiResponse<PagedResult<ArtistListDto>>> GetAllAsync(PageRequest page, string? search);
    Task<ApiResponse<ArtistDetailDto>> GetByIdAsync(int artistId, PackageLevel userPackage);
}