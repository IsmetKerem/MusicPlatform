using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.User;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class ProfileController : Controller
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokenStore;

    public ProfileController(IApiClient api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<ProfileDto>("/api/profile");

        if (!result.Success || result.Data is null)
        {
            TempData["Error"] = result.Message ?? "Profil yüklenemedi.";
            return RedirectToAction("Index", "Home");
        }

        var profile = result.Data;

        return View(new ProfilePageViewModel
        {
            Profile = profile,
            Update = new UpdateProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName  = profile.LastName,
                BirthDate = profile.BirthDate
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Lütfen alanları kontrol edin.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.PutAsync<ProfileDto>("/api/profile", new UpdateProfileDto
        {
            FirstName = model.FirstName,
            LastName  = model.LastName,
            BirthDate = model.BirthDate
        });

        if (result.Success)
            TempData["Success"] = result.Message ?? "Profil güncellendi.";
        else
            TempData["Error"] = result.Message ?? "Güncelleme başarısız.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Lütfen şifre alanlarını kontrol edin.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.PostAsync<object>("/api/profile/change-password", new ChangePasswordDto
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword     = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        });

        if (!result.Success)
        {
            TempData["Error"] = result.Message ?? "Şifre değiştirilemedi.";
            return RedirectToAction(nameof(Index));
        }
        
        _tokenStore.Clear();

        TempData["Success"] = "Şifreniz güncellendi. Lütfen yeni şifrenizle giriş yapın.";
        return RedirectToAction("Login", "Auth");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Dosya seçilmedi.";
            return RedirectToAction(nameof(Index));
        }

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/profile/avatar")
        {
            Content = content
        };

        var response = await _api.SendRawAsync(request);

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode
                ? "Profil fotoğrafı güncellendi."
                : "Fotoğraf yüklenemedi.";

        response.Dispose();
        return RedirectToAction(nameof(Index));
    }
}