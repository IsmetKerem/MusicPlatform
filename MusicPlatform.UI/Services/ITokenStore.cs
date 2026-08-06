namespace MusicPlatform.UI.Services;

public interface ITokenStore
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    bool IsAuthenticated { get; }

    void Save(string accessToken, string refreshToken, DateTime accessTokenExpiresAt);
    void Clear();
}