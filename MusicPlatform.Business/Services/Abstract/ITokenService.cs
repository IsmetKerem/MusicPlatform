using MusicPlatform.Entity.Concrete;
using MusicPlatform.Shared.DTOs.Auth;

namespace MusicPlatform.Business.Services.Abstract;

public interface ITokenService
{
    Task<TokenResponseDto> CreateTokenAsync(AppUser user, string? ipAddress = null);
    Task<TokenResponseDto?> RefreshAsync(string refreshToken, string? ipAddress = null);
    Task RevokeAllForUserAsync(int userId);
}