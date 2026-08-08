using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.UI.Models;

public class SongListViewModel
{
    public PagedResult<SongListDto> Result { get; set; } = new();
    public List<GenreListDto> Genres { get; set; } = new();

    public string? Search { get; set; }
    public int? GenreId { get; set; }
    public int? RequiredPackage { get; set; }
    public bool OnlyPlayable { get; set; }
    public int SortBy { get; set; }

    public string SortLabel => SortBy switch
    {
        1 => "Sanatçı adı",
        2 => "En çok dinlenen",
        3 => "En yeni",
        4 => "Süre",
        _ => "Şarkı adı"
    };
}

public class SongDetailViewModel
{
    public SongListDto Song { get; set; } = null!;
    public List<RecommendedSongDto> Recommendations { get; set; } = new();
    public List<SongListDto> MoreFromArtist { get; set; } = new();
}