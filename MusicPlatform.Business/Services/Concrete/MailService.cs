using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MusicPlatform.Business.Options;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;

namespace MusicPlatform.Business.Services.Concrete;

public class MailService : IMailService
{
    private readonly MailOptions _options;
    private readonly AppDbContext _context;
    private readonly ILogger<MailService> _logger;

    public MailService(IOptions<MailOptions> options, AppDbContext context, ILogger<MailService> logger)
    {
        _options = options.Value;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string toEmail, string subject, string htmlBody, string templateName, int? userId = null)
    {
        var log = new EmailLog
        {
            ToEmail      = toEmail,
            Subject      = subject,
            TemplateName = templateName,
            UserId       = userId
        };

        try
        {
            if (_options.DevelopmentMode)
            {
                _logger.LogInformation(
                    "[MAIL-DEV] Alıcı: {To} | Konu: {Subject} | Şablon: {Template}",
                    toEmail, subject, templateName);

                log.IsSent = true;
                log.SentAt = DateTime.UtcNow;
            }
            else
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var client = new SmtpClient();

  
                client.CheckCertificateRevocation = false;

                var secureOption = _options.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_options.Host, _options.Port, secureOption);
                await client.AuthenticateAsync(_options.Username, _options.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                log.IsSent = true;
                log.SentAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            log.IsSent       = false;
            log.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            _logger.LogError(ex, "Mail gönderilemedi: {To} / {Template}", toEmail, templateName);
        }

        _context.EmailLogs.Add(log);
        await _context.SaveChangesAsync();

        return log.IsSent;
    }
}