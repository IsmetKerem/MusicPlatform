using System.ComponentModel.DataAnnotations;
using MusicPlatform.Shared.DTOs.Playlist;

namespace MusicPlatform.UI.Models;

public class PlaylistPageViewModel
{
    public List<PlaylistListDto> Playlists { get; set; } = new();
    public CreatePlaylistViewModel New { get; set; } = new();
}

public class CreatePlaylistViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Playlist adı gerekli.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Ad 2-150 karakter olmalı.")]
    [Display(Name = "Playlist adı")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Herkese açık")]
    public bool IsPublic { get; set; }
}