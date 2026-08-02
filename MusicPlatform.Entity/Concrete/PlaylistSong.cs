namespace MusicPlatform.Entity.Concrete;

public class PlaylistSong
{
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;

    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public int SortOrder { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}