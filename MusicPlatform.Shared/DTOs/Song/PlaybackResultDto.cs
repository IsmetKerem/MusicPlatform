namespace MusicPlatform.Shared.DTOs.Song;

public class PlaybackResultDto
{
    public int SongId { get; set; }
    public string Title { get; set; } = null!;
    public string ArtistName { get; set; } = null!;
    public string StreamUrl { get; set; } = null!;
    public int DurationInSeconds { get; set; }
    public string? CoverImageUrl { get; set; }
}

public class PackageDeniedDto
{
    public int SongId { get; set; }
    public string SongTitle { get; set; } = null!;
    public int RequiredPackage { get; set; }
    public string RequiredPackageName { get; set; } = null!;
    public int UserPackage { get; set; }
    public string UserPackageName { get; set; } = null!;
    public string UpgradeUrl { get; set; } = "/paketler";
}