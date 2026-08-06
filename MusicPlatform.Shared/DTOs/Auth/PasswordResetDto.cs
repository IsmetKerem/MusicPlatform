namespace MusicPlatform.Shared.DTOs.Auth;

public class ForgotPasswordDto
{
    public string Email { get; set; } = null!;
}

public class ResetPasswordDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

public class ConfirmEmailDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
}