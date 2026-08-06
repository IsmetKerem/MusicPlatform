using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicPlatform.Business.Mail;
using MusicPlatform.Business.Options;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Business.Services.Concrete;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IMailService _mailService;
    private readonly IRecommendationService _recommendationService;
    private readonly MailOptions _mail;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext context,
        IMailService mailService,
        IRecommendationService recommendationService,
        IOptions<MailOptions> mail,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _mailService = mailService;
        _recommendationService = recommendationService;
        _mail = mail.Value;
        _logger = logger;
    }

    public async Task SendWelcomeAsync(int userId, string confirmToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        var url = $"{_mail.AppBaseUrl}/Auth/ConfirmEmail?userId={userId}&token={Uri.EscapeDataString(confirmToken)}";

        await _mailService.SendAsync(
            user.Email!, "MusicPlatform'a hoş geldin!",
            MailTemplates.Welcome(user.FullName, url),
            "Welcome", userId);
    }

    public async Task SendPasswordResetAsync(int userId, string resetToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        var url = $"{_mail.AppBaseUrl}/Auth/ResetPassword?userId={userId}&token={Uri.EscapeDataString(resetToken)}";

        await _mailService.SendAsync(
            user.Email!, "Şifre sıfırlama talebiniz",
            MailTemplates.PasswordReset(user.FullName, url),
            "PasswordReset", userId);
    }

    public async Task SendPasswordChangedAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        await _mailService.SendAsync(
            user.Email!, "Şifreniz değiştirildi",
            MailTemplates.PasswordChanged(user.FullName, DateTime.UtcNow),
            "PasswordChanged", userId);
    }

    public async Task SendPurchaseReceiptAsync(int userId, string transactionReference)
    {
        var purchase = await _context.PackagePurchases
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.TransactionReference == transactionReference);

        if (purchase is null) return;

        await _mailService.SendAsync(
            purchase.User.Email!,
            $"{purchase.PackageLevel} paketiniz aktifleşti",
            MailTemplates.PurchaseReceipt(
                purchase.User.FullName,
                purchase.PackageLevel.ToString(),
                purchase.Price,
                purchase.ExpiresAt ?? DateTime.UtcNow,
                purchase.TransactionReference),
            "PurchaseReceipt", userId);
    }

    public async Task SendUpgradeInvitationAsync(int userId, int songId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        var song = await _context.Songs.AsNoTracking().FirstOrDefaultAsync(s => s.Id == songId);
        if (user is null || song is null) return;

        var since = DateTime.UtcNow.AddDays(-1);
        var alreadySent = await _context.EmailLogs.AnyAsync(e =>
            e.UserId == userId &&
            e.TemplateName == "UpgradeInvitation" &&
            e.CreatedAt > since);

        if (alreadySent) return;

        await _mailService.SendAsync(
            user.Email!, "Bu şarkıyı kaçırma",
            MailTemplates.UpgradeInvitation(
                user.FullName, song.Title, song.RequiredPackage.ToString(),
                $"{_mail.AppBaseUrl}/Packages"),
            "UpgradeInvitation", userId);
    }

    public async Task SendNewDeviceLoginAsync(int userId, string ip)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        await _mailService.SendAsync(
            user.Email!, "Yeni cihazdan giriş yapıldı",
            MailTemplates.NewDeviceLogin(user.FullName, ip, DateTime.UtcNow),
            "NewDeviceLogin", userId);
    }

    public async Task SendExpiryRemindersAsync()
    {
        var target = DateTime.UtcNow.AddDays(3);
        var windowStart = target.Date;
        var windowEnd = windowStart.AddDays(1);

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.PackageLevel != PackageLevel.Basic &&
                        u.PackageExpiresAt >= windowStart &&
                        u.PackageExpiresAt < windowEnd)
            .ToListAsync();

        _logger.LogInformation("[JOB] Paket bitiş hatırlatması: {Count} kullanıcı", users.Count);

        foreach (var user in users)
        {
            var daysLeft = Math.Max(1, (int)(user.PackageExpiresAt!.Value - DateTime.UtcNow).TotalDays);

            await _mailService.SendAsync(
                user.Email!, $"Paketinizin bitmesine {daysLeft} gün kaldı",
                MailTemplates.PackageExpiring(
                    user.FullName, user.PackageLevel.ToString(), daysLeft,
                    $"{_mail.AppBaseUrl}/Packages"),
                "PackageExpiring", user.Id);
        }
    }

    public async Task SendWeeklyRecommendationsAsync()
    {
        var since = DateTime.UtcNow.AddDays(-30);

        var activeUserIds = await _context.ListeningHistories
            .Where(h => h.ListenedAt > since)
            .GroupBy(h => h.UserId)
            .Where(g => g.Count() >= 3)
            .Select(g => g.Key)
            .ToListAsync();

        _logger.LogInformation("[JOB] Haftalık öneri bülteni: {Count} kullanıcı", activeUserIds.Count);

        foreach (var userId in activeUserIds)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) continue;

            var recs = await _recommendationService.GetForUserAsync(userId, user.PackageLevel, 5);
            if (recs.Data is null || recs.Data.Count == 0) continue;

            var songs = recs.Data.Select(s => (s.Title, s.ArtistName)).ToList();

            await _mailService.SendAsync(
                user.Email!, "Bu hafta senin için seçtiklerimiz",
                MailTemplates.WeeklyRecommendations(user.FullName, songs, _mail.AppBaseUrl),
                "WeeklyRecommendations", userId);
        }
    }
}