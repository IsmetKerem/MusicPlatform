using MusicPlatform.Shared.Common;

namespace MusicPlatform.Shared.DTOs.Song;

public class SongFilterDto : PageRequest
{
    public string? Search { get; set; }
    public int? GenreId { get; set; }
    public int? ArtistId { get; set; }
    public int? AlbumId { get; set; }
    public int? RequiredPackage { get; set; }

    public bool OnlyPlayable { get; set; } = false;

    public SongSortBy SortBy { get; set; } = SongSortBy.Title;
}

public enum SongSortBy
{
    Title = 0,
    ArtistName = 1,
    MostPlayed = 2,
    Newest = 3,
    Duration = 4
}