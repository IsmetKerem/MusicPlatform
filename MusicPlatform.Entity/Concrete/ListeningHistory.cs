using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class ListeningHistory : BaseEntity
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    public int ListenedSeconds { get; set; }
    public bool IsCompleted { get; set; }
}