using MusicPlatform.Entity.Enums;

namespace MusicPlatform.Business.Services.Abstract;

public interface IPackageAuthorizationService
{
    bool CanAccess(PackageLevel userPackage, PackageLevel requiredPackage);

    Task<PackageCheckResult> CanUserPlaySongAsync(int userId, int songId);
}

public class PackageCheckResult
{
    public bool Allowed { get; set; }
    public bool SongExists { get; set; }
    public PackageLevel UserPackage { get; set; }
    public PackageLevel RequiredPackage { get; set; }
    public string SongTitle { get; set; } = string.Empty;
}