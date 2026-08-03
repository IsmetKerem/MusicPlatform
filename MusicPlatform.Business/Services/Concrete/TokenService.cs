using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicPlatform.Business.Options;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.DAL.Context;
using MusicPlatform.Entity.Concrete;
using MusicPlatform.Shared.DTOs.Auth;

namespace MusicPlatform.Business.Services.Concrete;

public class TokenService : ITokenService
{
    private readonly AppDbContext _context;
    private readonly JwtOptions _jwt;

    public TokenService(AppDbContext context, IOptions<JwtOptions> jwt)
    {
        _context = context;
        _jwt = jwt.Value;
    }

    public async Task<TokenResponseDto> CreateTokenAsync(AppUser user, string? ipAddress = null)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var accessToken = GenerateAccessToken(user, expires);

        var refresh = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync();

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refresh.Token,
            AccessTokenExpiresAt = expires,
            User = MapUser(user)
        };
    }

    public async Task<TokenResponseDto?> RefreshAsync(string refreshToken, string? ipAddress = null)
    {
        var stored = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null) return null;


        if (stored.IsRevoked)
        {
            await RevokeAllForUserAsync(stored.UserId);
            return null;
        }

        if (DateTime.UtcNow >= stored.ExpiresAt) return null;

        var user = stored.User;

        var newRefresh = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newRefresh.Token;

        _context.RefreshTokens.Add(newRefresh);
        await _context.SaveChangesAsync();

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        return new TokenResponseDto
        {
            AccessToken = GenerateAccessToken(user, expires),
            RefreshToken = newRefresh.Token,
            AccessTokenExpiresAt = expires,
            User = MapUser(user)
        };
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // ------------------------------------------------------------- helpers
    private string GenerateAccessToken(AppUser user, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("fullName", user.FullName),
            new("package", ((int)user.PackageLevel).ToString()),
            new("packageName", user.PackageLevel.ToString())
        };

        if (user.PackageExpiresAt.HasValue)
            claims.Add(new Claim("packageExpiresAt", user.PackageExpiresAt.Value.ToString("O")));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static UserInfoDto MapUser(AppUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email!,
        ProfileImageUrl = user.ProfileImageUrl,
        PackageName = user.PackageLevel.ToString(),
        PackageLevel = (int)user.PackageLevel,
        PackageExpiresAt = user.PackageExpiresAt
    };
}