using Microsoft.AspNetCore.Identity;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Auth;

namespace MusicPlatform.Business.Services.Concrete;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto dto, string? ip = null)
    {
        if (dto.Password != dto.ConfirmPassword)
            return ApiResponse<TokenResponseDto>.Fail("Şifreler eşleşmiyor.");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return ApiResponse<TokenResponseDto>.Fail("Bu e-posta adresi zaten kayıtlı.");

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PackageLevel = PackageLevel.Basic,   
            PackageExpiresAt = null,
            EmailConfirmed = true               
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return ApiResponse<TokenResponseDto>.Fail(
                "Kayıt başarısız.",
                result.Errors.Select(e => e.Description).ToList());

        var token = await _tokenService.CreateTokenAsync(user, ip);
        return ApiResponse<TokenResponseDto>.Ok(token, "Kayıt başarılı.");
    }

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto, string? ip = null)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return ApiResponse<TokenResponseDto>.Fail("E-posta veya şifre hatalı.");

        if (user.PackageExpiresAt.HasValue && user.PackageExpiresAt.Value < DateTime.UtcNow)
        {
            user.PackageLevel = PackageLevel.Basic;
            user.PackageExpiresAt = null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await _tokenService.CreateTokenAsync(user, ip);
        return ApiResponse<TokenResponseDto>.Ok(token, "Giriş başarılı.");
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshAsync(string refreshToken, string? ip = null)
    {
        var token = await _tokenService.RefreshAsync(refreshToken, ip);

        return token is null
            ? ApiResponse<TokenResponseDto>.Fail("Refresh token geçersiz veya süresi dolmuş.")
            : ApiResponse<TokenResponseDto>.Ok(token);
    }

    public async Task<ApiResponse> LogoutAsync(int userId)
    {
        await _tokenService.RevokeAllForUserAsync(userId);
        return ApiResponse.Ok("Çıkış yapıldı.");
    }
}