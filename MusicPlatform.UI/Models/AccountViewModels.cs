using System.ComponentModel.DataAnnotations;
using MusicPlatform.Shared.DTOs.Package;
using MusicPlatform.Shared.DTOs.User;

namespace MusicPlatform.UI.Models;

public class ProfilePageViewModel
{
    public ProfileDto Profile { get; set; } = null!;
    public UpdateProfileViewModel Update { get; set; } = new();
    public ChangePasswordViewModel Password { get; set; } = new();
}

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Ad gerekli.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Ad 2-80 karakter olmalı.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyad gerekli.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Soyad 2-80 karakter olmalı.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = null!;

    [DataType(DataType.Date)]
    [Display(Name = "Doğum tarihi")]
    public DateTime? BirthDate { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifre gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut şifre")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifre gerekli.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı gerekli.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}

public class PackagePageViewModel
{
    public List<PackageInfoDto> Packages { get; set; } = new();
    public List<PurchaseResultDto> History { get; set; } = new();
    public ProfileDto? Profile { get; set; }
}

public class PurchaseViewModel
{
    public int TargetPackageLevel { get; set; }

    [Display(Name = "Süre")]
    public int DurationInDays { get; set; } = 30;

    [Required(ErrorMessage = "Kart üzerindeki isim gerekli.")]
    [Display(Name = "Kart üzerindeki isim")]
    public string CardHolderName { get; set; } = null!;

    [Required(ErrorMessage = "Kart numarası gerekli.")]
    [Display(Name = "Kart numarası")]
    public string CardNumber { get; set; } = null!;

    [Required(ErrorMessage = "Ay gerekli.")]
    [Display(Name = "Ay")]
    public string ExpiryMonth { get; set; } = null!;

    [Required(ErrorMessage = "Yıl gerekli.")]
    [Display(Name = "Yıl")]
    public string ExpiryYear { get; set; } = null!;

    [Required(ErrorMessage = "CVV gerekli.")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = null!;
}