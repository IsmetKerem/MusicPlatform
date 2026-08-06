using Microsoft.EntityFrameworkCore;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Genre;

namespace MusicPlatform.Business.Services.Concrete;

public class GenreService : IGenreService
{
    private readonly AppDbContext _context;

    public GenreService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<GenreListDto>>> GetAllAsync()
    {
        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GenreListDto
            {
                Id          = g.Id,
                Name        = g.Name,
                Description = g.Description,
                ColorHex    = g.ColorHex,
                SongCount   = g.SongGenres.Count
            })
            .ToListAsync();

        return ApiResponse<List<GenreListDto>>.Ok(genres);
    }
}