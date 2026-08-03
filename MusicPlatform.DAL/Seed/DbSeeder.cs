using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.DAL.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<AppUser> userManager,
        string musicFolderPath,
        string coverFolderPath)
    {
        await context.Database.MigrateAsync();

        await SeedGenresAsync(context);
        await ScanMusicFolderAsync(context, musicFolderPath, coverFolderPath);
        await SeedUsersAsync(userManager);
        await SeedListeningHistoryAsync(context, userManager);
    }

    // ---------------------------------------------------------------- GENRES
    private static async Task SeedGenresAsync(AppDbContext context)
    {
        if (await context.Genres.AnyAsync()) return;

        await context.Genres.AddRangeAsync(
            new Genre { Name = "Pop",        ColorHex = "#E91E63", Description = "Geniş kitlelere hitap eden popüler müzik." },
            new Genre { Name = "Rap",        ColorHex = "#FF9800", Description = "Ritmik söz ve beat odaklı tür." },
            new Genre { Name = "R&B",        ColorHex = "#9C27B0", Description = "Ritim ve blues kökenli, groove ağırlıklı tür." },
            new Genre { Name = "Rock",       ColorHex = "#F44336", Description = "Gitar ağırlıklı, enerjik tür." },
            new Genre { Name = "Electronic", ColorHex = "#00BCD4", Description = "Elektronik prodüksiyon ağırlıklı müzik." },
            new Genre { Name = "Akustik",    ColorHex = "#8BC34A", Description = "Sade enstrümantasyon, canlı kayıt hissi." },
            new Genre { Name = "Arabesk",    ColorHex = "#607D8B", Description = "Duygusal anlatım ağırlıklı yerel tür." },
            new Genre { Name = "Jazz",       ColorHex = "#3F51B5", Description = "Doğaçlama ve swing ritimleri." }
        );

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Sanatçı adına göre tür ataması. Listede olmayan sanatçılar "Pop" sayılır.
    /// </summary>
    private static readonly Dictionary<string, string[]> ArtistGenreMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- Rap ---
        ["Şam"]               = new[] { "Rap" },
        ["Poizi"]             = new[] { "Rap" },
        ["Doğu Swag"]         = new[] { "Rap" },
        ["Eypio"]             = new[] { "Rap" },
        ["CRUSH"]             = new[] { "Rap" },
        ["Amo988"]            = new[] { "Rap" },
        ["Asil Gök"]          = new[] { "Rap" },

        // --- Arabesk / Arabesk-Rap ---
        ["Semicenk"]          = new[] { "Arabesk", "R&B" },
        ["Ceylan"]            = new[] { "Arabesk" },
        ["Mahsun Kırmızıgül"] = new[] { "Arabesk" },
        ["Aynur Polat"]       = new[] { "Arabesk" },
        ["Berkay"]            = new[] { "Arabesk", "Pop" },
        ["Burak Bulut"]       = new[] { "Arabesk", "Pop" },
        ["Demet Sağıroğlu"]   = new[] { "Arabesk", "Pop" },
        ["Serkan Nişancı"]    = new[] { "Arabesk", "Pop" },

        // --- Pop ---
        ["Gülşen"]            = new[] { "Pop", "Electronic" },
        ["Hadise"]            = new[] { "Pop", "Electronic" },
        ["Ozan Doğulu"]       = new[] { "Pop", "Electronic" },
        ["Demet Akalın"]      = new[] { "Pop" },
        ["Ajda Pekkan"]       = new[] { "Pop" },
        ["Sibel Can"]         = new[] { "Pop" },
        ["Erol Evgin"]        = new[] { "Pop" },
        ["Murat Dalkılıç"]    = new[] { "Pop" },
        ["Tan Taşçı"]         = new[] { "Pop" },
        ["Bahadır Tatlıöz"]   = new[] { "Pop" },
        ["Cem Belevi"]        = new[] { "Pop" },
        ["Derya Uluğ"]        = new[] { "Pop" },
        ["Mert Demir"]        = new[] { "Pop" },
        ["Reynmen"]           = new[] { "Pop" },
        ["manifest"]          = new[] { "Pop" },
        ["Ceren Sagu"]        = new[] { "Pop" },
        ["Mela Bedel"]        = new[] { "Pop" },
        ["Sıla Şahin"]        = new[] { "Pop" },
        ["Zeki Arkun"]        = new[] { "Pop" },

        // --- Pop / Akustik ---
        ["Yalın"]             = new[] { "Pop", "Akustik" },
        ["Fettah Can"]        = new[] { "Pop", "Akustik" },
        ["Elif Buse Doğan"]   = new[] { "Akustik" },

        // --- Rock / Alternatif ---
        ["Mabel Matiz"]       = new[] { "Pop", "Rock" },
        ["Nazan Öncel"]       = new[] { "Rock", "Pop" },
        ["Zeynep Casalini"]   = new[] { "Rock", "Pop" },
        ["Mavi Gri"]          = new[] { "Pop", "Rock" },

        // --- Jazz ---
        ["Peter Cincotti"]    = new[] { "Jazz" },
    };

    /// <summary>
    /// Şarkı başlığında geçen ipuçlarına göre ek tür ataması.
    /// </summary>
    private static readonly (string Keyword, string Genre)[] TitleGenreHints =
    {
        ("akustik",  "Akustik"),
        ("senfonik", "Akustik"),
        ("live",     "Akustik"),
        ("remix",    "Electronic"),
    };

    // ------------------------------------------------- MÜZİK KLASÖRÜ TARAMASI
    private static async Task ScanMusicFolderAsync(
        AppDbContext context, string musicFolderPath, string coverFolderPath)
    {
        if (await context.Songs.AnyAsync()) return;

        if (!Directory.Exists(musicFolderPath))
        {
            Console.WriteLine($"[SEED] Müzik klasörü bulunamadı: {musicFolderPath}");
            return;
        }

        Directory.CreateDirectory(coverFolderPath);

        var files = Directory.GetFiles(musicFolderPath, "*.mp3")
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                             .ToList();

        var genreIds = await context.Genres.ToDictionaryAsync(g => g.Name, g => g.Id);

        var artistCache = new Dictionary<string, Artist>(StringComparer.OrdinalIgnoreCase);
        var albumCache  = new Dictionary<string, Album>(StringComparer.OrdinalIgnoreCase);

        var packages = new[] { PackageLevel.Basic, PackageLevel.Gold, PackageLevel.Premium, PackageLevel.Elit };
        var index = 0;
        var coverCount = 0;

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            string title;
            string artistName;
            string? albumTitle;
            var duration = 180;
            string? coverUrl = null;

            try
            {
                using var tag = TagLib.File.Create(file);

                title = Clean(tag.Tag.Title) ?? PrettifyFileName(fileName);

                artistName = PrimaryArtist(
                    Clean(tag.Tag.FirstPerformer)
                    ?? Clean(tag.Tag.FirstAlbumArtist)
                    ?? "Bilinmeyen Sanatçı");

                albumTitle = Clean(tag.Tag.Album);

                if (albumTitle is not null &&
                    albumTitle.Equals("Unknown Album", StringComparison.OrdinalIgnoreCase))
                    albumTitle = null;

                if (tag.Properties.Duration.TotalSeconds > 1)
                    duration = (int)tag.Properties.Duration.TotalSeconds;

                coverUrl = ExtractCover(tag, fileName, coverFolderPath);
                if (coverUrl is not null) coverCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEED] '{fileName}' okunamadı ({ex.GetType().Name}), dosya adından devam ediliyor.");
                title      = PrettifyFileName(fileName);
                artistName = "Bilinmeyen Sanatçı";
                albumTitle = null;
            }

            if (!artistCache.TryGetValue(artistName, out var artist))
            {
                artist = new Artist
                {
                    Name = artistName,
                    Country = "Türkiye",
                    Bio = $"{artistName}, sistemde {{n}} şarkısıyla yer alıyor."
                };
                context.Artists.Add(artist);
                artistCache[artistName] = artist;
            }

            Album? album = null;
            if (albumTitle is not null)
            {
                var key = $"{artistName}||{albumTitle}";
                if (!albumCache.TryGetValue(key, out album))
                {
                    album = new Album { Title = albumTitle, Artist = artist };
                    context.Albums.Add(album);
                    albumCache[key] = album;
                }
            }

            var song = new Song
            {
                Title             = title,
                Artist            = artist,
                Album             = album,
                FileName          = fileName,
                DurationInSeconds = duration,
                CoverImageUrl     = coverUrl,
                RequiredPackage   = packages[index % packages.Length],
                PlayCount         = Random.Shared.Next(1_200, 95_000)
            };

            var chosen = new HashSet<string>();

            if (ArtistGenreMap.TryGetValue(artistName, out var mapped))
                foreach (var g in mapped) chosen.Add(g);

            foreach (var (keyword, genre) in TitleGenreHints)
                if (title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    chosen.Add(genre);

            if (chosen.Count == 0) chosen.Add("Pop");

            foreach (var g in chosen)
                if (genreIds.TryGetValue(g, out var gid))
                    song.SongGenres.Add(new SongGenre { GenreId = gid });

            context.Songs.Add(song);
            index++;
        }

        await context.SaveChangesAsync();

        foreach (var a in artistCache.Values)
        {
            var count = await context.Songs.CountAsync(s => s.ArtistId == a.Id);
            a.Bio = a.Bio!.Replace("{n}", count.ToString());
        }
        await context.SaveChangesAsync();

        Console.WriteLine($"[SEED] {files.Count} şarkı, {artistCache.Count} sanatçı, {albumCache.Count} albüm eklendi.");
        Console.WriteLine($"[SEED] Kapak görseli çıkarılan şarkı: {coverCount}/{files.Count}");
        Console.WriteLine("[SEED] Sanatçılar: " + string.Join(", ", artistCache.Keys.OrderBy(x => x)));

        var eksikTur = artistCache.Keys.Where(k => !ArtistGenreMap.ContainsKey(k)).ToList();
        if (eksikTur.Count > 0)
            Console.WriteLine("[SEED] Tür haritasında olmayan (Pop atandı): " + string.Join(", ", eksikTur));
    }

    private static string? ExtractCover(TagLib.File tag, string mp3FileName, string coverFolderPath)
    {
        var picture = tag.Tag.Pictures.FirstOrDefault();
        if (picture is null || picture.Data.Data.Length == 0) return null;

        var ext = picture.MimeType switch
        {
            "image/png"  => ".png",
            "image/webp" => ".webp",
            _            => ".jpg"
        };

        var coverName = Path.GetFileNameWithoutExtension(mp3FileName) + ext;
        var coverPath = Path.Combine(coverFolderPath, coverName);

        if (!File.Exists(coverPath))
            File.WriteAllBytes(coverPath, picture.Data.Data);

        return $"/covers/{coverName}";
    }


    private static string PrimaryArtist(string raw)
    {
        var separators = new[] { ",", " & ", " feat.", " feat ", " ft.", " ft ", " x ", " X ", " Feat" };

        var result = raw;
        foreach (var sep in separators)
        {
            var idx = result.IndexOf(sep, StringComparison.OrdinalIgnoreCase);
            if (idx > 0) result = result[..idx];
        }

        return result.Trim();
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim().Normalize(NormalizationForm.FormC);
    }

    private static string PrettifyFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ').Replace('_', ' ');
        return string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0]) + w[1..]));
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        var testUsers = new (string Email, string First, string Last, PackageLevel Pkg)[]
        {
            ("basic@music.com",   "Basic",   "Kullanıcı", PackageLevel.Basic),
            ("gold@music.com",    "Gold",    "Kullanıcı", PackageLevel.Gold),
            ("premium@music.com", "Premium", "Kullanıcı", PackageLevel.Premium),
            ("elit@music.com",    "Elit",    "Kullanıcı", PackageLevel.Elit)
        };

        foreach (var u in testUsers)
        {
            if (await userManager.FindByEmailAsync(u.Email) is not null) continue;

            var user = new AppUser
            {
                UserName         = u.Email,
                Email            = u.Email,
                EmailConfirmed   = true,
                FirstName        = u.First,
                LastName         = u.Last,
                PackageLevel     = u.Pkg,
                PackageExpiresAt = u.Pkg == PackageLevel.Basic ? null : DateTime.UtcNow.AddDays(30)
            };

            await userManager.CreateAsync(user, "Test123!");
        }
    }

    private static async Task SeedListeningHistoryAsync(AppDbContext context, UserManager<AppUser> userManager)
    {
        if (await context.ListeningHistories.AnyAsync()) return;

        var elit    = await userManager.FindByEmailAsync("elit@music.com");
        var premium = await userManager.FindByEmailAsync("premium@music.com");
        if (elit is null || premium is null) return;

        var songs = await context.Songs.OrderBy(s => s.Id).ToListAsync();
        if (songs.Count < 4) return;

        var rnd = Random.Shared;
        var history = new List<ListeningHistory>();

        var pair = new[] { songs[0], songs[1] };

        foreach (var user in new[] { elit, premium })
        {
            foreach (var s in pair)
                for (int i = 0; i < 4; i++)
                    history.Add(new ListeningHistory
                    {
                        UserId          = user.Id,
                        SongId          = s.Id,
                        ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(1, 20)),
                        ListenedSeconds = s.DurationInSeconds,
                        IsCompleted     = true
                    });

            for (int i = 0; i < 15; i++)
            {
                var s = songs[rnd.Next(songs.Count)];
                var listened = rnd.Next(20, Math.Max(25, s.DurationInSeconds));
                history.Add(new ListeningHistory
                {
                    UserId          = user.Id,
                    SongId          = s.Id,
                    ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(1, 30)),
                    ListenedSeconds = listened,
                    IsCompleted     = listened >= s.DurationInSeconds - 5
                });
            }
        }

        await context.ListeningHistories.AddRangeAsync(history);
        await context.SaveChangesAsync();
    }
}