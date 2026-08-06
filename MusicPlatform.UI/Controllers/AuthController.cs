using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Auth;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

public class AuthController : Controller
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokenStore;

    public AuthController(IApiClient api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_tokenStore.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<TokenResponseDto>("/api/auth/login", new LoginDto
        {
            Email = model.Email,
            Password = model.Password
        });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Giriş başarısız.");
            return View(model);
        }

        _tokenStore.Save(
            result.Data.AccessToken,
            result.Data.RefreshToken,
            result.Data.AccessTokenExpiresAt);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_tokenStore.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<TokenResponseDto>("/api/auth/register", new RegisterDto
        {
            FirstName       = model.FirstName,
            LastName        = model.LastName,
            Email           = model.Email,
            Password        = model.Password,
            ConfirmPassword = model.ConfirmPassword
        });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Kayıt başarısız.");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        _tokenStore.Save(
            result.Data.AccessToken,
            result.Data.RefreshToken,
            result.Data.AccessTokenExpiresAt);

        TempData["Success"] = "Hoş geldin! E-posta adresine bir doğrulama bağlantısı gönderdik.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _api.PostAsync<object>("/api/auth/forgot-password",
            new ForgotPasswordDto { Email = model.Email });

        TempData["Success"] = "E-posta adresiniz kayıtlıysa şifre sıfırlama bağlantısı gönderildi.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(int userId, string token)
    {
        if (userId == 0 || string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Geçersiz bağlantı.";
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostAsync<object>("/api/auth/reset-password", new ResetPasswordDto
        {
            UserId          = model.UserId,
            Token           = model.Token,
            NewPassword     = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Şifre sıfırlanamadı.");
            return View(model);
        }

        TempData["Success"] = "Şifreniz güncellendi, giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        var result = await _api.PostAsync<object>("/api/auth/confirm-email",
            new ConfirmEmailDto { UserId = userId, Token = token });

        if (result.Success)
            TempData["Success"] = result.Message ?? "E-posta adresiniz doğrulandı.";
        else
            TempData["Error"] = result.Message ?? "Doğrulama başarısız.";

        return RedirectToAction(_tokenStore.IsAuthenticated ? "Index" : nameof(Login),
                                _tokenStore.IsAuthenticated ? "Home" : "Auth");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (_tokenStore.IsAuthenticated)
            await _api.PostAsync<object>("/api/auth/logout");

        _tokenStore.Clear();
        return RedirectToAction(nameof(Login));
    }
}