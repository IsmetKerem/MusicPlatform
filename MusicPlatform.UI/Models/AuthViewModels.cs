using System.ComponentModel.DataAnnotations;

namespace MusicPlatform.UI.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta adresi gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad gerekli.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Ad 2-80 karakter olmalı.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyad gerekli.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Soyad 2-80 karakter olmalı.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "E-posta adresi gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gerekli.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı gerekli.")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta adresi gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;
}

public class ResetPasswordViewModel
{
    public int UserId { get; set; }
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifre gerekli.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı gerekli.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}