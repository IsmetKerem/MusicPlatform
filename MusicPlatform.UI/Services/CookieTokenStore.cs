using Microsoft.AspNetCore.Http;

namespace MusicPlatform.UI.Services;

/// <summary>
/// Token'lari HttpOnly cookie'de saklar.
/// localStorage kullanmiyoruz: XSS ile okunabilir.
/// HttpOnly cookie'ye JavaScript erisemez.
/// </summary>
public class CookieTokenStore : ITokenStore
{
    private const string AccessTokenKey  = "mp_at";
    private const string RefreshTokenKey = "mp_rt";

    private readonly IHttpContextAccessor _accessor;

    public CookieTokenStore(IHttpContextAccessor accessor) => _accessor = accessor;

    private HttpContext? Context => _accessor.HttpContext;

    public string? AccessToken  => Context?.Request.Cookies[AccessTokenKey];
    public string? RefreshToken => Context?.Request.Cookies[RefreshTokenKey];

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public void Save(string accessToken, string refreshToken, DateTime accessTokenExpiresAt)
    {
        if (Context is null) return;

        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure   = Context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,

            Expires  = DateTimeOffset.UtcNow.AddDays(7),
            Path     = "/"
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure   = Context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(7),
            Path     = "/"
        };

        Context.Response.Cookies.Append(AccessTokenKey, accessToken, accessOptions);
        Context.Response.Cookies.Append(RefreshTokenKey, refreshToken, refreshOptions);
    }

    public void Clear()
    {
        if (Context is null) return;

        Context.Response.Cookies.Delete(AccessTokenKey);
        Context.Response.Cookies.Delete(RefreshTokenKey);
    }
}