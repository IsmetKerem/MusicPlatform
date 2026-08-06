using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Shared.DTOs.Playlist;

public class PlaylistListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublic { get; set; }
    public int SongCount { get; set; }
    public int TotalDurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlaylistDetailDto : PlaylistListDto
{
    public List<SongListDto> Songs { get; set; } = new();
}

public class CreatePlaylistDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = false;
}