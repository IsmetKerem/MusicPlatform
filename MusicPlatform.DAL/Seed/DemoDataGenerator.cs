using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.DAL.Seed;


public static class DemoDataGenerator
{
    private static readonly (string Name, string[] Genres, int Weight)[] Profiles =
    {
        ("rapci",     new[] { "Rap" },                     25),
        ("popcu",     new[] { "Pop", "Electronic" },       30),
        ("arabeskci", new[] { "Arabesk", "R&B" },          20),
        ("rockcu",    new[] { "Rock", "Pop" },             15),
        ("akustikci", new[] { "Akustik", "Pop", "Jazz" },  10)
    };

    public static async Task GenerateAsync(
        AppDbContext context, UserManager<AppUser> userManager, int userCount = 60)
    {
        // Zaten üretilmişse tekrar çalışma
        if (await context.Users.CountAsync(u => u.Email!.EndsWith("@demo.local")) > 0)
        {
            Console.WriteLine("[DEMO] Demo kullanıcılar zaten mevcut, atlanıyor.");
            return;
        }

        var songs = await context.Songs
            .Include(s => s.SongGenres).ThenInclude(sg => sg.Genre)
            .ToListAsync();

        if (songs.Count == 0)
        {
            Console.WriteLine("[DEMO] Şarkı yok, demo veri üretilemedi.");
            return;
        }

        var rnd = new Random(42); 
        var packages = new[] { PackageLevel.Basic, PackageLevel.Gold, PackageLevel.Premium, PackageLevel.Elit };
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
                PackageExpiresAt = package == PackageLevel.Basic ? null : DateTime.UtcNow.AddDays(rnd.Next(5, 60)),
                CreatedAt        = DateTime.UtcNow.AddDays(-rnd.Next(30, 180)),
                LastLoginAt      = DateTime.UtcNow.AddDays(-rnd.Next(0, 7))
            };

            var created = await userManager.CreateAsync(user, "Demo123!");
            if (!created.Succeeded) continue;

            var core = songs
                .Where(s => s.SongGenres.Any(sg => profile.Genres.Contains(sg.Genre.Name)))
                .ToList();

            if (core.Count == 0) core = songs;

            var favorites = core.OrderBy(_ => rnd.Next()).Take(rnd.Next(6, 15)).ToList();

            foreach (var song in favorites)
            {
                var playCount = rnd.Next(2, 9);

                for (var p = 0; p < playCount; p++)
                {
                    var listened = rnd.NextDouble() < 0.75
                        ? song.DurationInSeconds                       // tamamladı
                        : rnd.Next(15, Math.Max(20, song.DurationInSeconds / 2)); // yarıda bıraktı

                    histories.Add(new ListeningHistory
                    {
                        UserId          = user.Id,
                        SongId          = song.Id,
                        ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(0, 45)).AddMinutes(-rnd.Next(0, 1440)),
                        ListenedSeconds = listened,
                        IsCompleted     = listened >= song.DurationInSeconds - 5
                    });
                }
            }

            var outside = songs.Except(favorites).OrderBy(_ => rnd.Next()).Take(rnd.Next(2, 6));

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

            foreach (var song in favorites.Take(rnd.Next(2, 5)))
                context.Favorites.Add(new Favorite { UserId = user.Id, SongId = song.Id });
        }

        await context.ListeningHistories.AddRangeAsync(histories);
        await context.SaveChangesAsync();

        Console.WriteLine($"[DEMO] {userCount} kullanıcı, {histories.Count} dinleme kaydı üretildi.");
    }

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