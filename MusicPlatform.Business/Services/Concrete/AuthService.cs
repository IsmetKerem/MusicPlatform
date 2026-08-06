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
    private readonly INotificationService _notification;

    public AuthService(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        INotificationService notification)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _notification = notification;
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
        
        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _notification.SendWelcomeAsync(user.Id, confirmToken);

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
        var isNewDevice = user.LastLoginAt.HasValue && ip is not null;

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        if (isNewDevice)
            await _notification.SendNewDeviceLoginAsync(user.Id, ip!);

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
    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);


        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _notification.SendPasswordResetAsync(user.Id, token);
        }

        return ApiResponse.Ok("E-posta adresiniz kayıtlıysa şifre sıfırlama bağlantısı gönderildi.");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return ApiResponse.Fail("Şifreler eşleşmiyor.");

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null) return ApiResponse.Fail("Geçersiz bağlantı.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return ApiResponse.Fail("Bağlantı geçersiz veya süresi dolmuş.",
                result.Errors.Select(e => e.Description).ToList());

        await _tokenService.RevokeAllForUserAsync(user.Id);
        await _notification.SendPasswordChangedAsync(user.Id);

        return ApiResponse.Ok("Şifreniz güncellendi, giriş yapabilirsiniz.");
    }

    public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null) return ApiResponse.Fail("Geçersiz bağlantı.");

        if (user.EmailConfirmed) return ApiResponse.Ok("E-posta adresiniz zaten doğrulanmış.");

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);

        return result.Succeeded
            ? ApiResponse.Ok("E-posta adresiniz doğrulandı.")
            : ApiResponse.Fail("Bağlantı geçersiz veya süresi dolmuş.");
    }
}