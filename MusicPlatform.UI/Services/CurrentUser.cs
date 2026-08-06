using System.IdentityModel.Tokens.Jwt;

namespace MusicPlatform.UI.Services;

/// <summary>
/// Token'i cozup icindeki claim'leri okur. API'ye ekstra istek atmadan
/// kullanici adi/paketi gibi bilgilere erisilir.
/// </summary>
public class CurrentUser
{
    private readonly ITokenStore _tokenStore;
    private JwtSecurityToken? _token;
    private bool _parsed;

    public CurrentUser(ITokenStore tokenStore) => _tokenStore = tokenStore;

    public bool IsAuthenticated => Token is not null && Token.ValidTo > DateTime.UtcNow.AddSeconds(-30);

    public int Id           => int.TryParse(Claim("nameid") ?? Claim("sub"), out var id) ? id : 0;
    public string FullName  => Claim("fullName") ?? "Kullanici";
    public string Email     => Claim("email") ?? "";
    public int PackageLevel => int.TryParse(Claim("package"), out var p) ? p : 1;
    public string PackageName => Claim("packageName") ?? "Basic";

    public bool CanPlay(int requiredPackage) => PackageLevel >= requiredPackage;

    private JwtSecurityToken? Token
    {
        get
        {
            if (_parsed) return _token;
            _parsed = true;

            var raw = _tokenStore.AccessToken;
            if (string.IsNullOrEmpty(raw)) return null;

            try
            {
                _token = new JwtSecurityTokenHandler().ReadJwtToken(raw);
            }
            catch
            {
                _token = null;
            }

            return _token;
        }
    }

    private string? Claim(string type) =>
        Token?.Claims.FirstOrDefault(c => c.Type == type)?.Value;
}