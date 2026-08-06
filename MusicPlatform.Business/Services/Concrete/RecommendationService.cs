using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicPlatform.Business.ML;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Enums;
using MusicPlatform.Shared.Common;
using MusicPlatform.Shared.DTOs.Song;

namespace MusicPlatform.Business.Services.Concrete;

public class RecommendationService : IRecommendationService
{
    private const int MinListenSeconds = 30;

    private const int MinCoOccurrence = 2;

    private readonly AppDbContext _context;
    private readonly IPackageAuthorizationService _packageAuth;
    private readonly ILogger<RecommendationService> _logger;
    private readonly MatrixFactorizationRecommender _mlRecommender;

    public RecommendationService(
        AppDbContext context,
        IPackageAuthorizationService packageAuth,
        ILogger<RecommendationService> logger, MatrixFactorizationRecommender mlRecommender)
    {
        _context = context;
        _packageAuth = packageAuth;
        _logger = logger;
        _mlRecommender = mlRecommender;
    }


    public async Task<ApiResponse<List<RecommendedSongDto>>> GetSimilarToSongAsync(
        int songId, PackageLevel userPackage, int count = 6, int? excludeUserId = null)
    {
        var song = await _context.Songs
            .AsNoTracking()
            .Include(s => s.SongGenres)
            .FirstOrDefaultAsync(s => s.Id == songId);

        if (song is null)
            return ApiResponse<List<RecommendedSongDto>>.Fail("Şarkı bulunamadı.");

        var listeners = await _context.ListeningHistories
            .AsNoTracking()
            .Where(h => h.SongId == songId && h.ListenedSeconds >= MinListenSeconds)
            .Select(h => h.UserId)
            .Distinct()
            .ToListAsync();

        var results = new List<RecommendedSongDto>();

        if (listeners.Count > 0)
        {
            var coOccurring = await _context.ListeningHistories
                .AsNoTracking()
                .Where(h => listeners.Contains(h.UserId)
                            && h.SongId != songId
                            && h.ListenedSeconds >= MinListenSeconds)
                .GroupBy(h => h.SongId)
                .Select(g => new
                {
                    SongId = g.Key,
                    SharedListeners = g.Select(x => x.UserId).Distinct().Count()
                })
                .Where(x => x.SharedListeners >= MinCoOccurrence)
                .OrderByDescending(x => x.SharedListeners)
                .Take(count * 3)
                .ToListAsync();

            if (coOccurring.Count > 0)
            {
                var ids = coOccurring.Select(c => c.SongId).ToList();
                var candidates = await LoadSongsAsync(ids);

                foreach (var c in coOccurring)
                {
                    if (!candidates.TryGetValue(c.SongId, out var candidate)) continue;

                    var score = (double)c.SharedListeners / listeners.Count;

                    results.Add(MapRecommended(candidate, userPackage, score,
                        $"{song.Title} dinleyenlerin %{score * 100:F0}'ı bunu da dinledi",
                        "CoOccurrence"));
                }
            }
        }

        if (results.Count < count)
        {
            var genreIds = song.SongGenres.Select(sg => sg.GenreId).ToList();
            var alreadyPicked = results.Select(r => r.Id).Append(songId).ToList();

            var byGenre = await _context.Songs
                .AsNoTracking()
                .Where(s => !alreadyPicked.Contains(s.Id)
                            && s.SongGenres.Any(sg => genreIds.Contains(sg.GenreId)))
                .OrderByDescending(s => s.PlayCount)
                .Take(count - results.Count)
                .Select(s => s.Id)
                .ToListAsync();

            var fill = await LoadSongsAsync(byGenre);

            foreach (var s in fill.Values)
                results.Add(MapRecommended(s, userPackage, 0.3,
                    "Benzer türde popüler", "Genre"));
        }

        return ApiResponse<List<RecommendedSongDto>>.Ok(
            results.Take(count).ToList());
    }


    public async Task<ApiResponse<List<RecommendedSongDto>>> GetPersonalizedAsync(
        int userId, PackageLevel userPackage, int count = 10)
    {
        var myHistory = await _context.ListeningHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId && h.ListenedSeconds >= MinListenSeconds)
            .GroupBy(h => h.SongId)
            .Select(g => new { SongId = g.Key, Plays = g.Count() })
            .ToListAsync();

        var mySongIds = myHistory.Select(h => h.SongId).ToList();

        if (mySongIds.Count == 0)
            return await GetColdStartAsync(userPackage, count);

        var similarUsers = await _context.ListeningHistories
            .AsNoTracking()
            .Where(h => h.UserId != userId
                        && mySongIds.Contains(h.SongId)
                        && h.ListenedSeconds >= MinListenSeconds)
            .GroupBy(h => h.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Overlap = g.Select(x => x.SongId).Distinct().Count()
            })
            .Where(x => x.Overlap >= 2)
            .OrderByDescending(x => x.Overlap)
            .Take(40)
            .ToListAsync();

        var results = new List<RecommendedSongDto>();

        if (similarUsers.Count > 0)
        {
            var neighborIds = similarUsers.Select(s => s.UserId).ToList();

            var weights = similarUsers.ToDictionary(s => s.UserId, s => (double)s.Overlap);

            var candidates = await _context.ListeningHistories
                .AsNoTracking()
                .Where(h => neighborIds.Contains(h.UserId)
                            && !mySongIds.Contains(h.SongId)
                            && h.ListenedSeconds >= MinListenSeconds)
                .Select(h => new { h.SongId, h.UserId })
                .Distinct()
                .ToListAsync();

            var scored = candidates
                .GroupBy(c => c.SongId)
                .Select(g => new
                {
                    SongId = g.Key,
                    Score  = g.Sum(x => weights.GetValueOrDefault(x.UserId, 1)),
                    Voters = g.Count()
                })
                .Where(x => x.Voters >= MinCoOccurrence)
                .OrderByDescending(x => x.Score)
                .Take(count * 2)
                .ToList();

            if (scored.Count > 0)
            {
                var maxScore = scored.Max(s => s.Score);
                var songs = await LoadSongsAsync(scored.Select(s => s.SongId).ToList());

                foreach (var s in scored)
                {
                    if (!songs.TryGetValue(s.SongId, out var song)) continue;

                    results.Add(MapRecommended(song, userPackage,
                        maxScore > 0 ? s.Score / maxScore : 0,
                        $"Senin gibi dinleyen {s.Voters} kişi bunu seviyor",
                        "CoOccurrence"));
                }
            }
        }

        if (results.Count < count)
        {
            var myTopGenres = await _context.ListeningHistories
                .AsNoTracking()
                .Where(h => h.UserId == userId && h.ListenedSeconds >= MinListenSeconds)
                .SelectMany(h => h.Song.SongGenres)
                .GroupBy(sg => sg.GenreId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync();

            var exclude = results.Select(r => r.Id).Concat(mySongIds).ToList();

            var byGenre = await _context.Songs
                .AsNoTracking()
                .Where(s => !exclude.Contains(s.Id)
                            && s.SongGenres.Any(sg => myTopGenres.Contains(sg.GenreId)))
                .OrderByDescending(s => s.PlayCount)
                .Take(count - results.Count)
                .Select(s => s.Id)
                .ToListAsync();

            var fill = await LoadSongsAsync(byGenre);

            foreach (var s in fill.Values)
                results.Add(MapRecommended(s, userPackage, 0.25,
                    "Sevdiğin türlerden", "Genre"));
        }

        if (results.Count < count)
        {
            var exclude = results.Select(r => r.Id).Concat(mySongIds).ToList();

            var popular = await _context.Songs
                .AsNoTracking()
                .Where(s => !exclude.Contains(s.Id))
                .OrderByDescending(s => s.PlayCount)
                .Take(count - results.Count)
                .Select(s => s.Id)
                .ToListAsync();

            var fill = await LoadSongsAsync(popular);

            foreach (var s in fill.Values)
                results.Add(MapRecommended(s, userPackage, 0.1,
                    "Platformda popüler", "Popular"));
        }

        _logger.LogInformation(
            "[REC] Kullanıcı {UserId}: {Total} öneri ({CoOcc} davranışsal)",
            userId, results.Count, results.Count(r => r.Source == "CoOccurrence"));

        return ApiResponse<List<RecommendedSongDto>>.Ok(results.Take(count).ToList());
    }

    public async Task<ApiResponse<List<SongListDto>>> GetForUserAsync(
        int userId, PackageLevel userPackage, int count = 10)
    {
        var personalized = await GetPersonalizedAsync(userId, userPackage, count);

        var list = personalized.Data?
            .Where(r => r.CanPlay)          
            .Cast<SongListDto>()
            .ToList() ?? new List<SongListDto>();

        return ApiResponse<List<SongListDto>>.Ok(list);
    }


    private async Task<ApiResponse<List<RecommendedSongDto>>> GetColdStartAsync(
        PackageLevel userPackage, int count)
    {
        var topPerGenre = await _context.Genres
            .AsNoTracking()
            .Select(g => g.SongGenres
                .OrderByDescending(sg => sg.Song.PlayCount)
                .Select(sg => sg.SongId)
                .FirstOrDefault())
            .Where(id => id != 0)
            .ToListAsync();

        var songs = await LoadSongsAsync(topPerGenre);

        var results = songs.Values
            .OrderByDescending(s => s.PlayCount)
            .Take(count)
            .Select(s => MapRecommended(s, userPackage, 0.5,
                "Başlamak için popüler seçkiler", "Popular"))
            .ToList();

        return ApiResponse<List<RecommendedSongDto>>.Ok(results);
    }

    private async Task<Dictionary<int, Entity.Concrete.Song>> LoadSongsAsync(List<int> ids)
    {
        if (ids.Count == 0) return new Dictionary<int, Entity.Concrete.Song>();

        return await _context.Songs
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Include(s => s.Artist)
            .Include(s => s.Album)
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .ToDictionaryAsync(s => s.Id);
    }

    private RecommendedSongDto MapRecommended(
        Entity.Concrete.Song s, PackageLevel userPackage,
        double score, string reason, string source) => new()
    {
        Id                  = s.Id,
        Title               = s.Title,
        ArtistId            = s.ArtistId,
        ArtistName          = s.Artist.Name,
        AlbumTitle          = s.Album?.Title,
        CoverImageUrl       = s.CoverImageUrl,
        DurationInSeconds   = s.DurationInSeconds,
        DurationDisplay     = TimeSpan.FromSeconds(s.DurationInSeconds).ToString(@"m\:ss"),
        PlayCount           = s.PlayCount,
        RequiredPackage     = (int)s.RequiredPackage,
        RequiredPackageName = s.RequiredPackage.ToString(),
        CanPlay             = _packageAuth.CanAccess(userPackage, s.RequiredPackage),
        Genres              = s.SongGenres.Select(sg => sg.Genre.Name).ToList(),
        Score               = Math.Round(score, 3),
        Reason              = reason,
        Source              = source
    };

    public async Task<bool> TrainModelAsync()
    {
        var data = await _context.ListeningHistories
            .AsNoTracking()
            .Where(h => h.ListenedSeconds >= MinListenSeconds)
            .GroupBy(h => new { h.UserId, h.SongId })
            .Select(g => new ML.ListeningRecord
            {
                UserId = g.Key.UserId,
                SongId = g.Key.SongId,
                Label  = Math.Min(g.Count(), 5)
            })
            .ToListAsync();

        var trained = _mlRecommender.Train(data);

        _logger.LogInformation(
            trained
                ? "[ML] Model eğitildi: {Rows} kayıt"
                : "[ML] Yetersiz veri ({Rows} kayıt), co-occurrence kullanılacak",
            data.Count);

        return trained;
    }
}