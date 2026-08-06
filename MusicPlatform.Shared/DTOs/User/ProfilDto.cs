namespace MusicPlatform.Shared.DTOs.User;

public class ProfileDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public DateTime? BirthDate { get; set; }

    public int PackageLevel { get; set; }
    public string PackageName { get; set; } = null!;
    public DateTime? PackageExpiresAt { get; set; }
    public int? RemainingDays { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ProfileStatsDto Stats { get; set; } = new();
}

public class ProfileStatsDto
{
    public int TotalListens { get; set; }
    public int DistinctSongs { get; set; }
    public int FavoriteCount { get; set; }
    public int PlaylistCount { get; set; }
    public int TotalMinutes { get; set; }
    public string? TopGenre { get; set; }
    public string? TopArtist { get; set; }
}

public class UpdateProfileDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}