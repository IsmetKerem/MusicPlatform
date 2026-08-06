using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Package;

namespace MusicPlatform.Business.Services.Abstract;

public interface IPackageService
{
    Task<ApiResponse<List<PackageInfoDto>>> GetCatalogAsync(int userId);
    Task<ApiResponse<PurchaseResultDto>> PurchaseAsync(int userId, PurchaseRequestDto dto);
    Task<ApiResponse<List<PurchaseResultDto>>> GetHistoryAsync(int userId);
}