using Microsoft.AspNetCore.Http;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.User;

namespace MusicPlatform.Business.Services.Abstract;

public interface IUserService
{
    Task<ApiResponse<ProfileDto>> GetProfileAsync(int userId);
    Task<ApiResponse<ProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<ApiResponse<string>> UploadAvatarAsync(int userId, IFormFile file);
}