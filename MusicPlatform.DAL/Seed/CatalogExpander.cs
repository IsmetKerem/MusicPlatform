using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.DAL.Seed;

/// <summary>
/// Katalogu genişletir: App_Data/catalog.json dosyasındaki sanatçı, albüm ve
/// şarkı tanımlarını veritabanına ekler.
///
/// Ses dosyaları paylaşılır — her yeni şarkı, klasörde zaten var olan
/// MP3'lerden birine sırayla atanır. Böylece katalogdaki her şarkı gerçekten
/// çalar; sadece ses içeriği tekrar eder.
///
/// Kapak görselleri wwwroot/covers/ altındaki gen-XXX.jpg dosyalarından gelir.
/// </summary>
public static class CatalogExpander
{
    private const int TargetMinimumSongCount = 100;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task ExpandAsync(AppDbContext context, string contentRootPath)
    {
        var existingCount = await context.Songs.CountAsync();

        if (existingCount >= TargetMinimumSongCount)
        {
            Console.WriteLine($"[EXPAND] Katalogda zaten {existingCount} şarkı var, atlanıyor.");
            return;
        }

        var jsonPath = Path.Combine(contentRootPath, "App_Data", "catalog.json");

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[EXPAND] catalog.json bulunamadı: {jsonPath}");
            return;
        }

        var musicFolder = Path.Combine(contentRootPath, "App_Data", "Music");

        var mp3Files = Directory.Exists(musicFolder)
            ? Directory.GetFiles(musicFolder, "*.mp3")
                       .Select(Path.GetFileName)
                       .Where(f => f is not null)
                       .Select(f => f!)
                       .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                       .ToList()
            : new List<string>();

        if (mp3Files.Count == 0)
        {
            Console.WriteLine("[EXPAND] MP3 dosyası yok, katalog genişletilemedi.");
            return;
        }

        CatalogFile? catalog;
        await using (var stream = File.OpenRead(jsonPath))
            catalog = await JsonSerializer.DeserializeAsync<CatalogFile>(stream, JsonOptions);

        if (catalog is null)
        {
            Console.WriteLine("[EXPAND] catalog.json okunamadı.");
            return;
        }

        // ---------------------------------------------------- Mevcut veriler
        var genreIds = await context.Genres.ToDictionaryAsync(g => g.Name, g => g.Id);

        var existingArtists = await context.Artists
            .ToDictionaryAsync(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);

        var existingSongTitles = await context.Songs
            .Select(s => s.Title + "||" + s.Artist.Name)
            .ToListAsync();

        var seenSongs = new HashSet<string>(existingSongTitles, StringComparer.OrdinalIgnoreCase);

        // ---------------------------------------------------------- Sanatçılar
        var artistCache = new Dictionary<string, Artist>(existingArtists, StringComparer.OrdinalIgnoreCase);
        var newArtists = 0;

        foreach (var a in catalog.Artists)
        {
            if (artistCache.ContainsKey(a.Name)) continue;

            var artist = new Artist
            {
                Name      = a.Name,
                Country   = a.Country,
                DebutYear = a.DebutYear,
                Bio       = a.Bio
            };

            context.Artists.Add(artist);
            artistCache[a.Name] = artist;
            newArtists++;
        }

        // ------------------------------------------------------------ Albümler
        var albumCache = new Dictionary<string, Album>(StringComparer.OrdinalIgnoreCase);
        var newAlbums = 0;

        foreach (var al in catalog.Albums)
        {
            if (!artistCache.TryGetValue(al.ArtistName, out var artist)) continue;

            var key = $"{al.ArtistName}||{al.Title}";
            if (albumCache.ContainsKey(key)) continue;

            var album = new Album
            {
                Title       = al.Title,
                Artist      = artist,
                ReleaseDate = new DateTime(al.ReleaseYear, 1, 1)
            };

            context.Albums.Add(album);
            albumCache[key] = album;
            newAlbums++;
        }

        // ------------------------------------------------------------- Şarkılar
        var packages = new[]
        {
            PackageLevel.Basic, PackageLevel.Gold,
            PackageLevel.Premium, PackageLevel.Elit
        };

        var index = existingCount;   // paket dağılımı mevcut şarkıların devamından
        var mp3Index = 0;
        var newSongs = 0;

        foreach (var s in catalog.Songs)
        {
            var dupeKey = $"{s.Title}||{s.ArtistName}";
            if (seenSongs.Contains(dupeKey)) continue;
            seenSongs.Add(dupeKey);

            if (!artistCache.TryGetValue(s.ArtistName, out var artist)) continue;

            Album? album = null;
            if (!string.IsNullOrEmpty(s.AlbumTitle))
                albumCache.TryGetValue($"{s.ArtistName}||{s.AlbumTitle}", out album);

            var song = new Song
            {
                Title             = s.Title,
                Artist            = artist,
                Album             = album,

                // Ses dosyası paylaşılıyor: mevcut MP3'ler sırayla atanıyor
                FileName          = mp3Files[mp3Index % mp3Files.Count],

                CoverImageUrl     = $"/covers/{s.CoverFile}",
                DurationInSeconds = s.DurationSeconds,
                PlayCount         = s.PlayCount,
                ReleaseDate       = new DateTime(s.ReleaseYear, 1, 1),
                RequiredPackage   = packages[index % packages.Length]
            };

            foreach (var genreName in s.Genres)
                if (genreIds.TryGetValue(genreName, out var gid))
                    song.SongGenres.Add(new SongGenre { GenreId = gid });

            context.Songs.Add(song);

            mp3Index++;
            index++;
            newSongs++;
        }

        await context.SaveChangesAsync();

        var total = await context.Songs.CountAsync();

        Console.WriteLine($"[EXPAND] +{newArtists} sanatçı, +{newAlbums} albüm, +{newSongs} şarkı");
        Console.WriteLine($"[EXPAND] Katalog toplamı: {total} şarkı, " +
                          $"{mp3Files.Count} farklı ses dosyası üzerinden çalıyor.");
    }

    // ------------------------------------------------------- JSON modelleri
    private class CatalogFile
    {
        public List<ArtistEntry> Artists { get; set; } = new();
        public List<AlbumEntry> Albums { get; set; } = new();
        public List<SongEntry> Songs { get; set; } = new();
    }

    private class ArtistEntry
    {
        public string Name { get; set; } = null!;
        public string Country { get; set; } = "Türkiye";
        public int DebutYear { get; set; }
        public string Bio { get; set; } = "";
    }

    private class AlbumEntry
    {
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public int ReleaseYear { get; set; }
    }

    private class SongEntry
    {
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public string? AlbumTitle { get; set; }
        public List<string> Genres { get; set; } = new();
        public string CoverFile { get; set; } = null!;
        public int DurationSeconds { get; set; }
        public long PlayCount { get; set; }
        public int ReleaseYear { get; set; }
    }
}
