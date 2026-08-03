namespace MusicPlatform.Shared.DTOs.Auth;

public class TokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public string PackageName { get; set; } = null!;
    public int PackageLevel { get; set; }
    public DateTime? PackageExpiresAt { get; set; }
}