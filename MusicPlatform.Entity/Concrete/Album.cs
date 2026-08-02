using MusicPlatform.Entity.Common;

namespace MusicPlatform.Entity.Concrete;

public class Album : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }

    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public ICollection<Song> Songs { get; set; } = new List<Song>();
}