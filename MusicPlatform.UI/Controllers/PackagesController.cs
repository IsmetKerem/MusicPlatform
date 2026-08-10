using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Shared.DTOs.Auth;
using MusicPlatform.Shared.DTOs.Package;
using MusicPlatform.Shared.DTOs.User;
using MusicPlatform.UI.Filters;
using MusicPlatform.UI.Models;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Controllers;

[RequireLogin]
public class PackagesController : Controller
{
    private readonly IApiClient _api;
    private readonly ITokenStore _tokenStore;

    public PackagesController(IApiClient api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    public async Task<IActionResult> Index()
    {
        var packagesTask = _api.GetAsync<List<PackageInfoDto>>("/api/packages");
        var historyTask  = _api.GetAsync<List<PurchaseResultDto>>("/api/packages/history");
        var profileTask  = _api.GetAsync<ProfileDto>("/api/profile");

        await Task.WhenAll(packagesTask, historyTask, profileTask);

        return View(new PackagePageViewModel
        {
            Packages = packagesTask.Result.Data ?? new(),
            History  = historyTask.Result.Data ?? new(),
            Profile  = profileTask.Result.Data
        });
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int level)
    {
        var result = await _api.GetAsync<List<PackageInfoDto>>("/api/packages");
        var package = result.Data?.FirstOrDefault(p => p.Level == level);

        if (package is null || !package.CanUpgradeTo)
        {
            TempData["Error"] = "Bu pakete geçiş yapılamıyor.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Package = package;
        return View(new PurchaseViewModel { TargetPackageLevel = level, DurationInDays = 30 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(PurchaseViewModel model)
    {
        var packagesResult = await _api.GetAsync<List<PackageInfoDto>>("/api/packages");
        var package = packagesResult.Data?.FirstOrDefault(p => p.Level == model.TargetPackageLevel);

        if (!ModelState.IsValid)
        {
            ViewBag.Package = package;
            return View(model);
        }

        var result = await _api.PostAsync<PurchaseResultDto>("/api/packages/purchase", new PurchaseRequestDto
        {
            TargetPackageLevel = model.TargetPackageLevel,
            DurationInDays     = model.DurationInDays,
            CardHolderName     = model.CardHolderName,
            CardNumber         = model.CardNumber,
            ExpiryMonth        = model.ExpiryMonth,
            ExpiryYear         = model.ExpiryYear,
            Cvv                = model.Cvv
        });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Ödeme alınamadı.");
            ViewBag.Package = package;
            return View(model);
        }


        if (result.Data.RequiresTokenRefresh)
            await RefreshTokenAsync();

        TempData["Success"] =
            $"{result.Data.PackageName} paketiniz aktifleştirildi. " +
            $"İşlem no: {result.Data.TransactionReference}";

        return RedirectToAction(nameof(Success), new { reference = result.Data.TransactionReference });
    }

    public async Task<IActionResult> Success(string reference)
    {
        var result = await _api.GetAsync<List<PurchaseResultDto>>("/api/packages/history");
        var purchase = result.Data?.FirstOrDefault(p => p.TransactionReference == reference);

        if (purchase is null) return RedirectToAction(nameof(Index));

        return View(purchase);
    }


    private async Task RefreshTokenAsync()
    {
        var refreshToken = _tokenStore.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken)) return;

        var result = await _api.PostAsync<TokenResponseDto>("/api/auth/refresh",
            new RefreshTokenRequestDto { RefreshToken = refreshToken });

        if (result.Success && result.Data is not null)
        {
            _tokenStore.Save(
                result.Data.AccessToken,
                result.Data.RefreshToken,
                result.Data.AccessTokenExpiresAt);
        }
    }
}