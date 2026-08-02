using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class Playlist : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublic { get; set; } = false;

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}