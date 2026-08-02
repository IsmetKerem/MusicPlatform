using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}