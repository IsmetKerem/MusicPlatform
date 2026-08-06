namespace MusicPlatform.Shared.DTOs.Artist;

public class ArtistListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Country { get; set; }
    public int SongCount { get; set; }
    public long TotalPlayCount { get; set; }
}

public class ArtistDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public string? Country { get; set; }
    public int? DebutYear { get; set; }
    public int SongCount { get; set; }
    public long TotalPlayCount { get; set; }
    public List<string> TopGenres { get; set; } = new();
    public List<AlbumBriefDto> Albums { get; set; } = new();
    public List<Song.SongListDto> Songs { get; set; } = new();
}

public class AlbumBriefDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int SongCount { get; set; }
}