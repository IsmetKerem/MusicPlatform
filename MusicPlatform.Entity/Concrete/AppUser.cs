using Microsoft.AspNetCore.Identity;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Entity.Concrete;

public class AppUser : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public DateTime? BirthDate { get; set; }

    public PackageLevel PackageLevel { get; set; } = PackageLevel.Basic;
    public DateTime? PackageExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    public ICollection<PackagePurchase> PackagePurchases { get; set; } = new List<PackagePurchase>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}