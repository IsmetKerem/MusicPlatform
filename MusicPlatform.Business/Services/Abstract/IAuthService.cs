using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Auth;

namespace MusicPlatform.Business.Services.Abstract;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto dto, string? ip = null);
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto, string? ip = null);
    Task<ApiResponse<TokenResponseDto>> RefreshAsync(string refreshToken, string? ip = null);
    Task<ApiResponse> LogoutAsync(int userId);
}