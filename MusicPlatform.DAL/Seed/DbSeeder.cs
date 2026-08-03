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
        await SeedArtistsAsync(context);
        await SeedAlbumsAsync(context);
        await SeedSongsAsync(context, musicFolderPath, coverFolderPath);
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
            new Genre { Name = "Arabesk",    ColorHex = "#607D8B", Description = "Duygusal anlatım ağırlıklı yerel tür." }
        );

        await context.SaveChangesAsync();
    }

    // --------------------------------------------------------------- ARTISTS
    private static async Task SeedArtistsAsync(AppDbContext context)
    {
        if (await context.Artists.AnyAsync()) return;

        var artists = new[]
        {
            new Artist { Name = "Alega",             Country = "Türkiye", DebutYear = 2019, Bio = "Alternatif rap sahnesinde öne çıkan isim." },
            new Artist { Name = "Ali Chapo",         Country = "Türkiye", DebutYear = 2018, Bio = "Sert anlatımlı rap parçalarıyla tanınıyor." },
            new Artist { Name = "AURA",              Country = "Türkiye", DebutYear = 2020, Bio = "Yeni nesil rap kuşağının üretken temsilcisi." },
            new Artist { Name = "BLOK3",             Country = "Türkiye", DebutYear = 2017, Bio = "Arabesk ve rap füzyonuyla geniş kitleye ulaşan sanatçı." },
            new Artist { Name = "Burak Bulut",       Country = "Türkiye", DebutYear = 2016, Bio = "Pop ve arabesk arasında gezinen vokalist." },
            new Artist { Name = "Can Demir",         Country = "Türkiye", DebutYear = 2021, Bio = "Akustik dokulu pop şarkıların yazarı." },
            new Artist { Name = "Derya Uluğ",        Country = "Türkiye", DebutYear = 2015, Bio = "Dans pop üretimleriyle listelerde yer alan şarkıcı." },
            new Artist { Name = "Duru",              Country = "Türkiye", DebutYear = 2022, Bio = "Sade prodüksiyonlu pop şarkılarıyla dikkat çekti." },
            new Artist { Name = "Emre Fel",          Country = "Türkiye", DebutYear = 2019, Bio = "R&B ve pop kesişiminde üreten sanatçı." },
            new Artist { Name = "Gülşen",            Country = "Türkiye", DebutYear = 1996, Bio = "Türk pop müziğinin uzun soluklu isimlerinden." },
            new Artist { Name = "Hadise",            Country = "Belçika", DebutYear = 2005, Bio = "Dans pop odaklı repertuvarıyla tanınan sanatçı." },
            new Artist { Name = "LVBEL C5",          Country = "Türkiye", DebutYear = 2020, Bio = "Trap ve pop-rap hattında üreten rapçi." },
            new Artist { Name = "Mabel Matiz",       Country = "Türkiye", DebutYear = 2011, Bio = "Alternatif pop ve edebi söz yazarlığıyla öne çıkıyor." },
            new Artist { Name = "manifest",          Country = "Türkiye", DebutYear = 2024, Bio = "Performans odaklı pop grubu." },
            new Artist { Name = "Oğuzhan Koç",       Country = "Türkiye", DebutYear = 2013, Bio = "Duygusal pop şarkılarıyla bilinen şarkıcı ve sunucu." },
            new Artist { Name = "POIZI",             Country = "Türkiye", DebutYear = 2021, Bio = "Yeni nesil trap sahnesinden bir isim." },
            new Artist { Name = "Reynmen",           Country = "Türkiye", DebutYear = 2019, Bio = "Dijital platformlarda yüksek izlenme sayılarına ulaşan sanatçı." },
            new Artist { Name = "Sefo",              Country = "Türkiye", DebutYear = 2018, Bio = "Melodik rap üretimleriyle tanınan sanatçı." },
            new Artist { Name = "Semicenk",          Country = "Türkiye", DebutYear = 2018, Bio = "Arabesk-rap türünün popüler isimlerinden." },
            new Artist { Name = "Simge",             Country = "Türkiye", DebutYear = 2011, Bio = "Pop müzik sahnesinin tanınan vokalisti." },
            new Artist { Name = "Soner Sarıkabadayı", Country = "Türkiye", DebutYear = 2008, Bio = "Şarkı yazarı ve yorumcu." }
        };

        await context.Artists.AddRangeAsync(artists);
        await context.SaveChangesAsync();
    }

    // ---------------------------------------------------------------- ALBUMS
    private static async Task SeedAlbumsAsync(AppDbContext context)
    {
        if (await context.Albums.AnyAsync()) return;

        var artistIds = await context.Artists.ToDictionaryAsync(a => a.Name, a => a.Id);

        await context.Albums.AddRangeAsync(
            new Album { Title = "BLOK3 Seçkisi",      ArtistId = artistIds["BLOK3"],      ReleaseDate = new DateTime(2023, 4, 10) },
            new Album { Title = "manifest Seçkisi",   ArtistId = artistIds["manifest"],   ReleaseDate = new DateTime(2025, 2, 14) },
            new Album { Title = "Derya Uluğ Seçkisi", ArtistId = artistIds["Derya Uluğ"], ReleaseDate = new DateTime(2022, 8, 5)  },
            new Album { Title = "Semicenk Seçkisi",   ArtistId = artistIds["Semicenk"],   ReleaseDate = new DateTime(2023, 11, 3) }
        );

        await context.SaveChangesAsync();
    }

    // ----------------------------------------------------------------- SONGS
    private static async Task SeedSongsAsync(AppDbContext context, string musicFolderPath, string coverFolderPath)
    {
        if (await context.Songs.AnyAsync()) return;

        var artistIds = await context.Artists.ToDictionaryAsync(a => a.Name, a => a.Id);
        var genreIds  = await context.Genres.ToDictionaryAsync(g => g.Name, g => g.Id);
        var albumIds  = await context.Albums.ToDictionaryAsync(a => a.Title, a => a.Id);

        var data = new (string Title, string Artist, string? Album, string File, PackageLevel Pkg, string[] Genres)[]
        {
            // ------------------------------ BASIC (7) ------------------------------
            ("Düzenli Şekilde Ölmek", "Alega",       null,                 "alega-duzenli-sekilde-olmek.mp3",  PackageLevel.Basic,   new[]{"Rap"}),
            ("Canavar",               "Ali Chapo",   null,                 "ali-chapo-canavar.mp3",            PackageLevel.Basic,   new[]{"Rap"}),
            ("Tuzak",                 "AURA",        null,                 "aura-tuzak.mp3",                   PackageLevel.Basic,   new[]{"Rap"}),
            ("Gel Dedim Geldin",      "Can Demir",   null,                 "can-demir-gel-dedim-geldin.mp3",   PackageLevel.Basic,   new[]{"Pop","Akustik"}),
            ("aşk şarkısı (değil)",   "Duru",        null,                 "duru-ask-sarkisi-degil.mp3",       PackageLevel.Basic,   new[]{"Pop","Akustik"}),
            ("Bir Güldün",            "Emre Fel",    null,                 "emre-fel-ft-funktakl-bir-guldun.mp3", PackageLevel.Basic, new[]{"R&B","Pop"}),
            ("Başımda Belalar",       "POIZI",       null,                 "poizi-basimda-belalar.mp3",        PackageLevel.Basic,   new[]{"Rap"}),

            // ------------------------------ GOLD (7) -------------------------------
            ("Kırgınım",              "BLOK3",       "BLOK3 Seçkisi",      "blok3-kirginim.mp3",               PackageLevel.Gold,    new[]{"Rap","Arabesk"}),
            ("Kusura Bakma",          "BLOK3",       "BLOK3 Seçkisi",      "blok3-kusura-bakma.mp3",           PackageLevel.Gold,    new[]{"Rap"}),
            ("Diva",                  "Burak Bulut", null,                 "burak-bulut-diva.mp3",             PackageLevel.Gold,    new[]{"Pop"}),
            ("Hani",                  "Derya Uluğ",  "Derya Uluğ Seçkisi", "derya-ulug-hani.mp3",              PackageLevel.Gold,    new[]{"Pop"}),
            ("Geçsin Yıllar",         "Oğuzhan Koç", null,                 "oguzhan-koc-merve-ozbey-gecsin-yillar.mp3", PackageLevel.Gold, new[]{"Pop","Akustik"}),
            ("Çıkmaz Bir Sokakta",    "Semicenk",    "Semicenk Seçkisi",   "semicenk-cikmaz-bir-sokakta.mp3",  PackageLevel.Gold,    new[]{"Arabesk","R&B"}),
            ("Kalpsiz Bir Serseri",   "Simge",       null,                 "simge-kalpsiz-bir-serseri.mp3",    PackageLevel.Gold,    new[]{"Pop","Akustik"}),

            // ----------------------------- PREMIUM (7) -----------------------------
            ("Napıyosun Mesela?",     "BLOK3",       "BLOK3 Seçkisi",      "blok3-napiyosun-mesela.mp3",       PackageLevel.Premium, new[]{"Rap"}),
            ("Şımarık",               "Derya Uluğ",  "Derya Uluğ Seçkisi", "derya-ulug-simarik.mp3",           PackageLevel.Premium, new[]{"Pop","Electronic"}),
            ("İtaat Yok",             "Gülşen",      null,                 "gulsen-itaat-yok.mp3",             PackageLevel.Premium, new[]{"Pop","Electronic"}),
            ("Gel Gel Gel",           "LVBEL C5",    null,                 "lvbel-c5-dystinct-gel-gel-gel.mp3",PackageLevel.Premium, new[]{"Rap","Pop"}),
            ("Rüya",                  "manifest",    "manifest Seçkisi",   "manifest-ruya.mp3",                PackageLevel.Premium, new[]{"Pop","R&B"}),
            ("Çatma Yarim",           "Reynmen",     null,                 "reynmen-catma-yarim.mp3",          PackageLevel.Premium, new[]{"Pop","Arabesk"}),
            ("Her İki Durumda",       "Soner Sarıkabadayı", null,          "soner-sarikabadayi-sefo-aerro-her-iki-durumda.mp3", PackageLevel.Premium, new[]{"Pop","Rap"}),

            // ------------------------------ ELIT (6) -------------------------------
            ("Ara Beni",              "Hadise",      null,                 "hadise-ara-beni.mp3",              PackageLevel.Elit,    new[]{"Pop","Electronic"}),
            ("Dağılıyorum Olaysız",   "Mabel Matiz", null,                 "mabel-matiz-dagiliyorum-olaysiz.mp3", PackageLevel.Elit, new[]{"Pop","Rock"}),
            ("Başrol Sensin",         "manifest",    "manifest Seçkisi",   "manifest-basrol-sensin.mp3",       PackageLevel.Elit,    new[]{"Pop"}),
            ("Toz Pembe",             "manifest",    "manifest Seçkisi",   "manifest-toz-pembe.mp3",           PackageLevel.Elit,    new[]{"Pop","Electronic"}),
            ("Yerinde Dur",           "Sefo",        null,                 "sefo-demet-akalin-yerinde-dur.mp3",PackageLevel.Elit,    new[]{"Rap","Pop"}),
            ("Üzülmedim Ki",          "Semicenk",    "Semicenk Seçkisi",   "semicenk-uzulmedim-ki.mp3",        PackageLevel.Elit,    new[]{"Arabesk","Pop"})
        };

        Directory.CreateDirectory(coverFolderPath);

        foreach (var d in data)
        {
            var fullPath = Path.Combine(musicFolderPath, d.File);

            var song = new Song
            {
                Title           = d.Title,
                ArtistId        = artistIds[d.Artist],
                AlbumId         = d.Album is null ? null : albumIds[d.Album],
                FileName        = d.File,
                RequiredPackage = d.Pkg,
                PlayCount       = Random.Shared.Next(1_200, 95_000)
            };

            ReadMetadata(fullPath, coverFolderPath, song);

            foreach (var g in d.Genres)
                song.SongGenres.Add(new SongGenre { GenreId = genreIds[g] });

            context.Songs.Add(song);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// MP3'ten süreyi okur ve gömülü kapak görselini diske çıkarır.
    /// Dosya yoksa veya etiket bozuksa sessizce varsayılanlarla devam eder.
    /// </summary>
    private static void ReadMetadata(string mp3Path, string coverFolderPath, Song song)
    {
        if (!File.Exists(mp3Path))
        {
            song.DurationInSeconds = 180;
            return;
        }

        try
        {
            using var tag = TagLib.File.Create(mp3Path);

            song.DurationInSeconds = (int)tag.Properties.Duration.TotalSeconds;

            var picture = tag.Tag.Pictures.FirstOrDefault();
            if (picture is not null && picture.Data.Data.Length > 0)
            {
                var ext = picture.MimeType switch
                {
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };

                var coverName = Path.GetFileNameWithoutExtension(mp3Path) + ext;
                var coverPath = Path.Combine(coverFolderPath, coverName);

                if (!File.Exists(coverPath))
                    File.WriteAllBytes(coverPath, picture.Data.Data);

                song.CoverImageUrl = $"/covers/{coverName}";
            }
        }
        catch
        {
            if (song.DurationInSeconds == 0)
                song.DurationInSeconds = 180;
        }
    }

    // ----------------------------------------------------------------- USERS
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

    // ------------------------------------------------------ LISTENING HISTORY
    private static async Task SeedListeningHistoryAsync(AppDbContext context, UserManager<AppUser> userManager)
    {
        if (await context.ListeningHistories.AnyAsync()) return;

        var elit    = await userManager.FindByEmailAsync("elit@music.com");
        var premium = await userManager.FindByEmailAsync("premium@music.com");
        if (elit is null || premium is null) return;

        var songs = await context.Songs.ToDictionaryAsync(s => s.FileName, s => s);
        var rnd = Random.Shared;
        var history = new List<ListeningHistory>();

        // Öneri motorunun yakalayacağı kasıtlı ilişki:
        // "Kırgınım" dinleyen "Çıkmaz Bir Sokakta"yı da dinliyor.
        var pair = new[] { "blok3-kirginim.mp3", "semicenk-cikmaz-bir-sokakta.mp3" };

        foreach (var user in new[] { elit, premium })
        {
            foreach (var file in pair)
            {
                if (!songs.TryGetValue(file, out var s)) continue;

                for (int i = 0; i < 4; i++)
                    history.Add(new ListeningHistory
                    {
                        UserId          = user.Id,
                        SongId          = s.Id,
                        ListenedAt      = DateTime.UtcNow.AddDays(-rnd.Next(1, 20)),
                        ListenedSeconds = s.DurationInSeconds,
                        IsCompleted     = true
                    });
            }

            var pool = songs.Values.ToList();
            for (int i = 0; i < 15; i++)
            {
                var s = pool[rnd.Next(pool.Count)];
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