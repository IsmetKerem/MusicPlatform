namespace MusicPlatform.Business.Options;

public class MailOptions
{
    public const string SectionName = "Mail";

    public string Host { get; set; } = null!;
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = "MusicPlatform";

    public string AppBaseUrl { get; set; } = "https://localhost:7001";

    public bool DevelopmentMode { get; set; } = false;
}