namespace MusicPlatform.Business.Services.Abstract;

public interface INotificationService
{
    Task SendWelcomeAsync(int userId, string confirmToken);
    Task SendPasswordResetAsync(int userId, string resetToken);
    Task SendPasswordChangedAsync(int userId);
    Task SendPurchaseReceiptAsync(int userId, string transactionReference);
    Task SendUpgradeInvitationAsync(int userId, int songId);
    Task SendNewDeviceLoginAsync(int userId, string ip);

    Task SendExpiryRemindersAsync();
    Task SendWeeklyRecommendationsAsync();
}