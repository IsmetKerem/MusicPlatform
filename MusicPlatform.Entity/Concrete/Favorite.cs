namespace MusicPlatform.Entity.Concrete;

public class Favorite
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}