namespace MusicPlatform.Shared.DTOs.Genre;

public class GenreListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ColorHex { get; set; }
    public int SongCount { get; set; }
}