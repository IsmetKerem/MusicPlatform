using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.DAL.Seed;

/// <summary>
/// Öneri motorunun anlamlı sonuç üretebilmesi ve profil istatistiklerinin
/// dolu görünmesi için sahte kullanıcı, dinleme geçmişi, favori ve playlist
/// oluşturur.
///
/// Tür bazlı "zevk profilleri" kullanır: aynı profildeki kullanıcılar benzer
/// şarkıları dinler, böylece co-occurrence gerçek bir örüntü yakalar.
/// </summary>
public static class DemoDataGenerator
{
    private static readonly (string Name, string[] Genres, int Weight)[] Profiles =
    {
        ("rapci",     new[] { "Rap" },                     25),
        ("popcu",     new[] { "Pop", "Electronic" },       30),
        ("arabeskci", new[] { "Arabesk", "R&B" },          20),
        ("rockcu",    new[] { "Rock", "Pop" },             15),
        ("akustikci", new[] { "Akustik", "Jazz" },         10)
    };

    private static readonly string[] PlaylistNames =
    {
        "Sabah Kahvesi", "Çalışırken", "Yolculuk", "Gece Modu", "Spor",
        "Favorilerim", "Keşfettiklerim", "Sakin Akşam", "Enerji", "Nostalji"
    };

    public static async Task GenerateAsync(
        AppDbContext context, UserManager<AppUser> userManager, int userCount = 80)
    {
        var songs = await context.Songs
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .ToListAsync();

        if (songs.Count == 0)
        {
            Console.WriteLine("[DEMO] Şarkı yok, demo veri üretilemedi.");
            return;
        }

        var rnd = new Random(42);   // sabit seed: her çalıştırmada aynı sonuç

        // ═══════════════════════════════════════════ 1. Demo kullanıcılar
        var demoExists = await context.Users.AnyAsync(u => u.Email!.EndsWith("@demo.local"));

        if (!demoExists)
        {
            await CreateDemoUsersAsync(context, userManager, songs, rnd, userCount);
        }
        else
        {
            Console.WriteLine("[DEMO] Demo kullanıcılar zaten mevcut.");
        }

        // ═══════════════════════════════════ 2. Gerçek hesaplara veri ver
        // Test kullanıcıları ve kayıt olan hesaplar boş görünmesin
        await EnrichRealUsersAsync(context, songs, rnd);
    }

    // ───────────────────────────────────────────────── Demo kullanıcılar
    private static async Task CreateDemoUsersAsync(
        AppDbContext context, UserManager<AppUser> userManager,
        List<Song> songs, Random rnd, int userCount)
    {
        var packages = new[]
        {
            PackageLevel.Basic, PackageLevel.Gold,
            PackageLevel.Premium, PackageLevel.Elit
        };

        var histories = new List<ListeningHistory>();

        for (var i = 1; i <= userCount; i++)
        {
            var profile = PickProfile(rnd);
            var package = packages[rnd.Next(packages.Length)];
            var email   = $"{profile.Name}{i}@demo.local";

            var user = new AppUser
            {
                UserName         = email,
                Email            = email,
                EmailConfirmed   = true,
                FirstName        = $"Demo{i}",
                LastName         = profile.Name,
                PackageLevel     = package,
                PackageExpiresAt = package == PackageLevel.Basic
                    ? null
                    : DateTime.UtcNow.AddDays(rnd.Next(5, 60)),
                CreatedAt   = DateTime.UtcNow.AddDays(-rnd.Next(30, 180)),
                LastLoginAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 7))
            };

            var created = await userManager.CreateAsync(user, "Demo123!");
            if (!created.Succeeded) continue;

            // Profiline uyan şarkılar: bu kullanıcının çekirdek repertuvarı
            var core = songs
                .Where(s => s.SongGenres.Any(sg => profile.Genres.Contains(sg.Genre.Name)))
                .ToList();

            if (core.Count == 0) core = songs;

            var favorites = core.OrderBy(_ => rnd.Next()).Take(rnd.Next(8, 20)).ToList();

            foreach (var song in favorites)
            {
                var playCount = rnd.Next(2, 9);

                for (var p = 0; p < playCount; p++)
                {
                    var listened = rnd.NextDouble() < 0.75
                        ? song.DurationInSeconds
                        : rnd.Next(15, Math.Max(20, song.DurationInSeconds / 2));

                    histories.Add(new ListeningHistory
                    {
                        UserId          = user.Id,
                        SongId          = song.Id,
                        ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(0, 45))
                                                         .AddMinutes(-rnd.Next(0, 1440)),
                        ListenedSeconds = listened,
                        IsCompleted     = listened >= song.DurationInSeconds - 5
                    });
                }
            }

            // Profil dışından rastgele şarkılar — gerçek hayattaki gürültü
            var outside = songs.Except(favorites).OrderBy(_ => rnd.Next()).Take(rnd.Next(3, 8));

            foreach (var song in outside)
            {
                var listened = rnd.Next(10, Math.Max(15, song.DurationInSeconds));
                histories.Add(new ListeningHistory
                {
                    UserId          = user.Id,
                    SongId          = song.Id,
                    ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(0, 45)),
                    ListenedSeconds = listened,
                    IsCompleted     = listened >= song.DurationInSeconds - 5
                });
            }

            foreach (var song in favorites.Take(rnd.Next(3, 8)))
                context.Favorites.Add(new Favorite { UserId = user.Id, SongId = song.Id });
        }

        await context.ListeningHistories.AddRangeAsync(histories);
        await context.SaveChangesAsync();

        Console.WriteLine($"[DEMO] {userCount} kullanıcı, {histories.Count} dinleme kaydı üretildi.");
    }

    // ─────────────────────────────────── Gerçek hesapları zenginleştirme
    /// <summary>
    /// @demo.local dışındaki hesaplara dinleme geçmişi, favori ve playlist
    /// ekler. Profil sayfası ve öneriler boş görünmesin diye.
    /// </summary>
    private static async Task EnrichRealUsersAsync(
        AppDbContext context, List<Song> songs, Random rnd)
    {
        var realUsers = await context.Users
            .Where(u => !u.Email!.EndsWith("@demo.local"))
            .ToListAsync();

        var histories = new List<ListeningHistory>();
        var enriched = 0;

        foreach (var user in realUsers)
        {
            // Zaten geçmişi varsa dokunma
            var hasHistory = await context.ListeningHistories.AnyAsync(h => h.UserId == user.Id);
            if (hasHistory) continue;

            enriched++;

            // Kullanıcının paketinde açık olan şarkılar
            var accessible = songs
                .Where(s => (int)s.RequiredPackage <= (int)user.PackageLevel)
                .ToList();

            if (accessible.Count == 0) accessible = songs;

            // Rastgele iki türü "sevdiği tür" yap — istatistikler anlamlı çıksın
            var allGenres = songs.SelectMany(s => s.SongGenres.Select(sg => sg.Genre.Name))
                                 .Distinct().ToList();
            var favGenres = allGenres.OrderBy(_ => rnd.Next()).Take(2).ToList();

            var core = accessible
                .Where(s => s.SongGenres.Any(sg => favGenres.Contains(sg.Genre.Name)))
                .ToList();

            if (core.Count < 5) core = accessible;

            var listenedSongs = core.OrderBy(_ => rnd.Next())
                                    .Take(Math.Min(core.Count, rnd.Next(14, 26)))
                                    .ToList();

            foreach (var song in listenedSongs)
            {
                var times = rnd.Next(2, 7);

                for (var p = 0; p < times; p++)
                {
                    var listened = rnd.NextDouble() < 0.8
                        ? song.DurationInSeconds
                        : rnd.Next(20, Math.Max(25, song.DurationInSeconds / 2));

                    histories.Add(new ListeningHistory
                    {
                        UserId          = user.Id,
                        SongId          = song.Id,
                        ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(0, 30))
                                                         .AddMinutes(-rnd.Next(0, 1440)),
                        ListenedSeconds = listened,
                        IsCompleted     = listened >= song.DurationInSeconds - 5
                    });
                }
            }

            // Favoriler
            var hasFavorites = await context.Favorites.AnyAsync(f => f.UserId == user.Id);
            if (!hasFavorites)
            {
                foreach (var song in listenedSongs.Take(rnd.Next(6, 12)))
                    context.Favorites.Add(new Favorite
                    {
                        UserId  = user.Id,
                        SongId  = song.Id,
                        AddedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 25))
                    });
            }

            // Playlistler
            var hasPlaylists = await context.Playlists.AnyAsync(p => p.UserId == user.Id);
            if (!hasPlaylists)
            {
                var names = PlaylistNames.OrderBy(_ => rnd.Next()).Take(rnd.Next(2, 5)).ToList();

                foreach (var plName in names)
                {
                    var playlist = new Playlist
                    {
                        UserId      = user.Id,
                        Name        = plName,
                        Description = null,
                        IsPublic    = rnd.NextDouble() < 0.4,
                        CreatedAt   = DateTime.UtcNow.AddDays(-rnd.Next(1, 40))
                    };

                    var picks = accessible.OrderBy(_ => rnd.Next())
                                          .Take(rnd.Next(5, 13))
                                          .ToList();

                    var order = 1;
                    foreach (var song in picks)
                    {
                        playlist.PlaylistSongs.Add(new PlaylistSong
                        {
                            SongId    = song.Id,
                            SortOrder = order++,
                            AddedAt   = DateTime.UtcNow.AddDays(-rnd.Next(0, 30))
                        });
                    }

                    context.Playlists.Add(playlist);
                }
            }
        }

        if (histories.Count > 0)
            await context.ListeningHistories.AddRangeAsync(histories);

        await context.SaveChangesAsync();

        if (enriched > 0)
            Console.WriteLine($"[DEMO] {enriched} gerçek hesaba geçmiş, favori ve playlist eklendi.");
    }

    // ─────────────────────────────────────────────────────────── yardımcı
    private static (string Name, string[] Genres, int Weight) PickProfile(Random rnd)
    {
        var total = Profiles.Sum(p => p.Weight);
        var roll = rnd.Next(total);
        var acc = 0;

        foreach (var p in Profiles)
        {
            acc += p.Weight;
            if (roll < acc) return p;
        }

        return Profiles[0];
    }
}
