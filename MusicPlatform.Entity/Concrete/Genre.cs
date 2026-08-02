using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class Genre : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ColorHex { get; set; }

    public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
}