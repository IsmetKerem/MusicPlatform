using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class Artist : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public string? Country { get; set; }
    public int? DebutYear { get; set; }

    public ICollection<Song> Songs { get; set; } = new List<Song>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}