namespace MusicPlatform.Business.Services.Abstract;

public interface IMailService
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody,
        string templateName, int? userId = null);
}