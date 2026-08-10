using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Artist;
using MusicPlatform.Shared.DTOs.Genre;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.UI.Models;

public class ArtistListViewModel
{
    public PagedResult<ArtistListDto> Result { get; set; } = new();
    public string? Search { get; set; }
}

public class GenreDetailViewModel
{
    public GenreListDto Genre { get; set; } = null!;
    public List<SongListDto> Songs { get; set; } = new();
}