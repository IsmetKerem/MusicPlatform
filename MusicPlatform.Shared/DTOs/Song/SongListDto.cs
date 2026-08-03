namespace MusicPlatform.Shared.DTOs.Song;

public class SongListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = null!;
    public string? AlbumTitle { get; set; }
    public string? CoverImageUrl { get; set; }
    public int DurationInSeconds { get; set; }
    public string DurationDisplay { get; set; } = null!;
    public long PlayCount { get; set; }

    public int RequiredPackage { get; set; }
    public string RequiredPackageName { get; set; } = null!;

    public bool CanPlay { get; set; }

    public List<string> Genres { get; set; } = new();
}