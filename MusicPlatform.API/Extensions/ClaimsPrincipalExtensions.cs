using System.Security.Claims;
using MusicPlatform.Entity.Enums;

namespace MusicPlatform.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static PackageLevel GetPackageLevel(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("package");
        return int.TryParse(value, out var level) && Enum.IsDefined(typeof(PackageLevel), level)
            ? (PackageLevel)level
            : PackageLevel.Basic;
    }
}