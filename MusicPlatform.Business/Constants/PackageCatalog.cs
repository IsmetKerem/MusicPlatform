using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Business.Constants;

public static class PackageCatalog
{
    public record PackageDefinition(
        PackageLevel Level,
        decimal MonthlyPrice,
        string Description,
        string[] Features);

    public static readonly IReadOnlyDictionary<PackageLevel, PackageDefinition> All =
        new Dictionary<PackageLevel, PackageDefinition>
        {
            [PackageLevel.Basic] = new(
                PackageLevel.Basic, 0m,
                "Ücretsiz başlangıç paketi.",
                new[] { "Basic katalogdaki şarkılar", "Reklamlı dinleme", "Standart ses kalitesi" }),

            [PackageLevel.Gold] = new(
                PackageLevel.Gold, 49.90m,
                "Daha geniş katalog, reklamsız dinleme.",
                new[] { "Basic + Gold katalog", "Reklamsız", "Sınırsız playlist", "Yüksek ses kalitesi" }),

            [PackageLevel.Premium] = new(
                PackageLevel.Premium, 89.90m,
                "Premium katalog ve kişisel öneriler.",
                new[] { "Basic + Gold + Premium katalog", "Reklamsız", "Kişiselleştirilmiş öneriler", "Çok yüksek ses kalitesi" }),

            [PackageLevel.Elit] = new(
                PackageLevel.Elit, 149.90m,
                "Tüm katalog, en yüksek kalite.",
                new[] { "Tüm katalog", "Reklamsız", "Kişiselleştirilmiş öneriler", "Kayıpsız ses kalitesi", "Öncelikli destek" })
        };

    public static decimal GetPrice(PackageLevel level) => All[level].MonthlyPrice;
}