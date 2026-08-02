using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class EmailLog : BaseEntity
{
    public string ToEmail { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string TemplateName { get; set; } = null!;

    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;

    public int? UserId { get; set; }
    public AppUser? User { get; set; }
}