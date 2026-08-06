using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.User;

namespace MusicPlatform.Business.Services.Concrete;

public class UserService : IUserService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxAvatarBytes = 2 * 1024 * 1024; // 2 MB

    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly string _avatarFolder;
    private readonly INotificationService _notification;

    public UserService(
        AppDbContext context,
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IConfiguration config, INotificationService notification)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _notification = notification;
        _avatarFolder = config["MusicSettings:ResolvedAvatarPath"]
                        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars");
    }

    public async Task<ApiResponse<ProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return ApiResponse<ProfileDto>.Fail("Kullanıcı bulunamadı.");

        var stats = await BuildStatsAsync(userId);
        return ApiResponse<ProfileDto>.Ok(Map(user, stats));
    }

    public async Task<ApiResponse<ProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<ProfileDto>.Fail("Kullanıcı bulunamadı.");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return ApiResponse<ProfileDto>.Fail("Ad ve soyad boş olamaz.");

        if (dto.BirthDate.HasValue && dto.BirthDate.Value > DateTime.UtcNow.AddYears(-6))
            return ApiResponse<ProfileDto>.Fail("Geçerli bir doğum tarihi giriniz.");

        user.FirstName = dto.FirstName.Trim();
        user.LastName  = dto.LastName.Trim();
        user.BirthDate = dto.BirthDate;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ApiResponse<ProfileDto>.Fail("Profil güncellenemedi.",
                result.Errors.Select(e => e.Description).ToList());

        var stats = await BuildStatsAsync(userId);
        return ApiResponse<ProfileDto>.Ok(Map(user, stats), "Profil güncellendi.");
    }

    public async Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return ApiResponse.Fail("Yeni şifreler eşleşmiyor.");

        if (dto.CurrentPassword == dto.NewPassword)
            return ApiResponse.Fail("Yeni şifre mevcut şifreyle aynı olamaz.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse.Fail("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return ApiResponse.Fail("Şifre değiştirilemedi.",
                result.Errors.Select(e => e.Description).ToList());

        await _tokenService.RevokeAllForUserAsync(userId);

        await _notification.SendPasswordChangedAsync(userId);
        return ApiResponse.Ok("Şifreniz güncellendi. Diğer cihazlardaki oturumlarınız kapatıldı.");
    }

    public async Task<ApiResponse<string>> UploadAvatarAsync(int userId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return ApiResponse<string>.Fail("Dosya seçilmedi.");

        if (file.Length > MaxAvatarBytes)
            return ApiResponse<string>.Fail("Dosya boyutu en fazla 2 MB olabilir.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return ApiResponse<string>.Fail("Sadece jpg, png ve webp yükleyebilirsiniz.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<string>.Fail("Kullanıcı bulunamadı.");

        Directory.CreateDirectory(_avatarFolder);

        var fileName = $"user_{userId}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_avatarFolder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            var oldName = Path.GetFileName(user.ProfileImageUrl);
            var oldPath = Path.Combine(_avatarFolder, oldName);
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }

        user.ProfileImageUrl = $"/avatars/{fileName}";
        await _userManager.UpdateAsync(user);

        return ApiResponse<string>.Ok(user.ProfileImageUrl, "Profil fotoğrafı güncellendi.");
    }

    private async Task<ProfileStatsDto> BuildStatsAsync(int userId)
    {
        var history = await _context.ListeningHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .Include(h => h.Song).ThenInclude(s => s.Artist)
            .Include(h => h.Song).ThenInclude(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .ToListAsync();

        return new ProfileStatsDto
        {
            TotalListens  = history.Count,
            DistinctSongs = history.Select(h => h.SongId).Distinct().Count(),
            TotalMinutes  = history.Sum(h => h.ListenedSeconds) / 60,
            FavoriteCount = await _context.Favorites.CountAsync(f => f.UserId == userId),
            PlaylistCount = await _context.Playlists.CountAsync(p => p.UserId == userId),

            TopArtist = history
                .GroupBy(h => h.Song.Artist.Name)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault(),

            TopGenre = history
                .SelectMany(h => h.Song.SongGenres.Select(sg => sg.Genre.Name))
                .GroupBy(n => n)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault()
        };
    }

    private static ProfileDto Map(AppUser u, ProfileStatsDto stats) => new()
    {
        Id               = u.Id,
        FirstName        = u.FirstName,
        LastName         = u.LastName,
        FullName         = u.FullName,
        Email            = u.Email!,
        ProfileImageUrl  = u.ProfileImageUrl,
        BirthDate        = u.BirthDate,
        PackageLevel     = (int)u.PackageLevel,
        PackageName      = u.PackageLevel.ToString(),
        PackageExpiresAt = u.PackageExpiresAt,
        RemainingDays    = u.PackageExpiresAt.HasValue
            ? Math.Max(0, (int)(u.PackageExpiresAt.Value - DateTime.UtcNow).TotalDays)
            : null,
        CreatedAt   = u.CreatedAt,
        LastLoginAt = u.LastLoginAt,
        Stats       = stats
    };
}