using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.UI.Models;

public class HomeViewModel
{
    public List<SongListDto> Popular { get; set; } = new();
    public List<RecommendedSongDto> Recommendations { get; set; } = new();
    public List<GenreListDto> Genres { get; set; } = new();
    public List<SongListDto> Newest { get; set; } = new();
}