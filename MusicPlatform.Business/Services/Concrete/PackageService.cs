using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Constants;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Package;

namespace MusicPlatform.Business.Services.Concrete;

public class PackageService : IPackageService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly INotificationService _notification;

    public PackageService(AppDbContext context, ITokenService tokenService, INotificationService notification)
    {
        _context = context;
        _tokenService = tokenService;
        _notification = notification;
    }

    public async Task<ApiResponse<List<PackageInfoDto>>> GetCatalogAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return ApiResponse<List<PackageInfoDto>>.Fail("Kullanıcı bulunamadı.");

        var counts = await _context.Songs
            .GroupBy(s => s.RequiredPackage)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync();

        var list = PackageCatalog.All.Values
            .OrderBy(p => (int)p.Level)
            .Select(p => new PackageInfoDto
            {
                Level        = (int)p.Level,
                Name         = p.Level.ToString(),
                MonthlyPrice = p.MonthlyPrice,
                Description  = p.Description,
                Features     = p.Features.ToList(),

                AccessibleSongCount = counts
                    .Where(c => (int)c.Level <= (int)p.Level)
                    .Sum(c => c.Count),

                IsCurrent    = user.PackageLevel == p.Level,
                CanUpgradeTo = (int)p.Level > (int)user.PackageLevel
            })
            .ToList();

        return ApiResponse<List<PackageInfoDto>>.Ok(list);
    }

    public async Task<ApiResponse<PurchaseResultDto>> PurchaseAsync(int userId, PurchaseRequestDto dto)
    {
        if (!Enum.IsDefined(typeof(PackageLevel), dto.TargetPackageLevel))
            return ApiResponse<PurchaseResultDto>.Fail("Geçersiz paket seviyesi.");

        var target = (PackageLevel)dto.TargetPackageLevel;

        if (target == PackageLevel.Basic)
            return ApiResponse<PurchaseResultDto>.Fail("Basic paket ücretsizdir, satın alınamaz.");

        if (dto.DurationInDays is < 30 or > 365)
            return ApiResponse<PurchaseResultDto>.Fail("Süre 30 ile 365 gün arasında olmalıdır.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return ApiResponse<PurchaseResultDto>.Fail("Kullanıcı bulunamadı.");

        var validation = ValidateFakeCard(dto);
        if (validation is not null)
            return ApiResponse<PurchaseResultDto>.Fail(validation);

        var monthlyPrice = PackageCatalog.GetPrice(target);
        var amount = Math.Round(monthlyPrice * dto.DurationInDays / 30m, 2);

        var now = DateTime.UtcNow;

        var startsAt = user.PackageLevel == target && user.PackageExpiresAt > now
            ? user.PackageExpiresAt!.Value
            : now;

        var purchase = new PackagePurchase
        {
            UserId               = userId,
            PackageLevel         = target,
            Price                = amount,
            DurationInDays       = dto.DurationInDays,
            Status               = PurchaseStatus.Completed,
            StartsAt             = startsAt,
            ExpiresAt            = startsAt.AddDays(dto.DurationInDays),
            TransactionReference = GenerateReference()
        };

        _context.PackagePurchases.Add(purchase);

        user.PackageLevel     = target;
        user.PackageExpiresAt = purchase.ExpiresAt;

        await _context.SaveChangesAsync();



        await _notification.SendPurchaseReceiptAsync(userId, purchase.TransactionReference);

        return ApiResponse<PurchaseResultDto>.Ok(new PurchaseResultDto
        {
            TransactionReference = purchase.TransactionReference,
            PackageName          = target.ToString(),
            AmountPaid           = amount,
            StartsAt             = purchase.StartsAt!.Value,
            ExpiresAt            = purchase.ExpiresAt!.Value,
            RequiresTokenRefresh = true
        }, $"{target} paketiniz aktifleştirildi.");
    }

    public async Task<ApiResponse<List<PurchaseResultDto>>> GetHistoryAsync(int userId)
    {
        var list = await _context.PackagePurchases
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PurchaseResultDto
            {
                TransactionReference = p.TransactionReference,
                PackageName          = p.PackageLevel.ToString(),
                AmountPaid           = p.Price,
                StartsAt             = p.StartsAt ?? p.CreatedAt,
                ExpiresAt            = p.ExpiresAt ?? p.CreatedAt,
                RequiresTokenRefresh = false
            })
            .ToListAsync();

        return ApiResponse<List<PurchaseResultDto>>.Ok(list);
    }



    private static string? ValidateFakeCard(PurchaseRequestDto dto)
    {
        var number = (dto.CardNumber ?? "").Replace(" ", "").Replace("-", "");

        if (string.IsNullOrWhiteSpace(dto.CardHolderName) || dto.CardHolderName.Trim().Length < 5)
            return "Kart üzerindeki isim geçersiz.";

        if (number.Length is < 15 or > 19 || !number.All(char.IsDigit))
            return "Kart numarası geçersiz.";

        if (!IsLuhnValid(number))
            return "Kart numarası doğrulanamadı.";

        if (!int.TryParse(dto.ExpiryMonth, out var month) || month is < 1 or > 12)
            return "Son kullanma ayı geçersiz.";

        if (!int.TryParse(dto.ExpiryYear, out var year))
            return "Son kullanma yılı geçersiz.";

        if (year < 100) year += 2000;

        var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
        if (expiry < DateTime.UtcNow.Date)
            return "Kartınızın son kullanma tarihi geçmiş.";

        if (dto.Cvv is null || dto.Cvv.Length is < 3 or > 4 || !dto.Cvv.All(char.IsDigit))
            return "CVV geçersiz.";

        return null;
    }

    private static bool IsLuhnValid(string number)
    {
        var sum = 0;
        var alternate = false;

        for (var i = number.Length - 1; i >= 0; i--)
        {
            var digit = number[i] - '0';

            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    private static string GenerateReference()
        => $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}