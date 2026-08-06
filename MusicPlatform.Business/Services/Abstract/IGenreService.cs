using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Genre;

namespace MusicPlatform.Business.Services.Abstract;

public interface IGenreService
{
    Task<ApiResponse<List<GenreListDto>>> GetAllAsync();
}