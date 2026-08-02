using MusicPlatform.Entity.Common;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Entity.Concrete;

public class Song : BaseEntity
{
    public string Title { get; set; } = null!;
    public int DurationInSeconds { get; set; }
    public string FileName { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public long PlayCount { get; set; } = 0;

    public PackageLevel RequiredPackage { get; set; } = PackageLevel.Basic;

    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public int? AlbumId { get; set; }
    public Album? Album { get; set; }

    public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
}