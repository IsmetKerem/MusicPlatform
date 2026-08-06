using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlatform.API.Extensions;
using MusicPlatform.Business.Services.Abstract;
using MusicPlatform.Shared.DTOs.Package;

namespace MusicPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;

    public PackagesController(IPackageService packageService) => _packageService = packageService;

    [HttpGet]
    public async Task<IActionResult> GetCatalog()
    {
        var result = await _packageService.GetCatalogAsync(User.GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseRequestDto dto)
    {
        var result = await _packageService.PurchaseAsync(User.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
        => Ok(await _packageService.GetHistoryAsync(User.GetUserId()));
}