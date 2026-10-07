using System.Security.Claims;
using ApplicationCore.Entities.Shipping;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Shipping;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Shipping;

namespace PublicApi.Controllers.Shipping;

[ApiController]
[Authorize]
[Route("api/shipping-profiles")]
public sealed class ShippingProfileController : ControllerBase
{
    private readonly IShippingProfileService _shippingProfileService;

    public ShippingProfileController(IShippingProfileService shippingProfileService)
    {
        _shippingProfileService = shippingProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var profiles = await _shippingProfileService.GetMyProfilesAsync(GetUserId());
        return Ok(Success(profiles.Select(ToResponse).ToList(), "Shipping profiles retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UpsertShippingProfileRequest request)
    {
        var profile = await _shippingProfileService.CreateAsync(
            GetUserId(), request.RecipientName, request.RecipientPhone, request.ShippingAddress, request.IsDefault);

        return StatusCode(StatusCodes.Status201Created,
            Success(ToResponse(profile), "Shipping profile created successfully.", StatusCodes.Status201Created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpsertShippingProfileRequest request)
    {
        var profile = await _shippingProfileService.UpdateAsync(
            GetUserId(), id, request.RecipientName, request.RecipientPhone, request.ShippingAddress, request.IsDefault);

        return Ok(Success(ToResponse(profile), "Shipping profile updated successfully."));
    }

    [HttpPatch("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        await _shippingProfileService.SetDefaultAsync(GetUserId(), id);
        return Ok(Success<object?>(null, "Default shipping profile updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _shippingProfileService.DeleteAsync(GetUserId(), id);
        return Ok(Success<object?>(null, "Shipping profile deleted successfully."));
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedException("User is not authenticated.");
        return userId;
    }

    private static ApiResponse<T> Success<T>(T data, string message, int statusCode = StatusCodes.Status200OK) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private static ShippingProfileResponse ToResponse(ShippingProfile profile) => new()
    {
        Id = profile.Id,
        RecipientName = profile.RecipientName,
        RecipientPhone = profile.RecipientPhone,
        ShippingAddress = profile.ShippingAddress,
        IsDefault = profile.IsDefault,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}
