namespace MusicPlatform.Shared.DTOs.Song;

public class RecommendedSongDto : SongListDto
{
    public double Score { get; set; }

    public string Reason { get; set; } = null!;

    public string Source { get; set; } = null!;
}