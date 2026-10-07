using System.Security.Claims;
using ApplicationCore.Entities.Catalog;
using ApplicationCore.Entities.Shopping;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Shopping;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Shopping;

namespace PublicApi.Controllers.Shopping;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var cart = await _cartService.GetMyCartAsync(GetUserId());
        return Ok(Success(ToResponse(cart), "Cart retrieved successfully."));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        var cart = await _cartService.AddItemAsync(GetUserId(), request.ProductId, request.Quantity);
        return Ok(Success(ToResponse(cart), "Product added to cart successfully."));
    }

    [HttpPatch("items/{productId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(Guid productId, UpdateCartItemRequest request)
    {
        var cart = await _cartService.UpdateItemQuantityAsync(GetUserId(), productId, request.Quantity);
        return Ok(Success(ToResponse(cart), "Cart item quantity updated successfully."));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        await _cartService.RemoveItemAsync(GetUserId(), productId);
        return Ok(Success<object?>(null, "Product removed from cart successfully."));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("User is not authenticated.");

        return userId;
    }

    private static ApiResponse<T> Success<T>(T data, string message) => new()
    {
        StatusCode = StatusCodes.Status200OK,
        Message = message,
        Data = data
    };

    private static CartResponse ToResponse(Cart? cart)
    {
        if (cart is null)
            return new CartResponse();

        var items = cart.Items.Select(item => new CartItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ImageUrl = item.Product.ThumbnailUrl,
            UnitPrice = item.Product.SellingPrice,
            Quantity = item.Quantity,
            LineTotal = item.Product.SellingPrice * item.Quantity,
            IsAvailable = item.Product.Status == ProductStatus.Active && item.Product.Category.IsActive
        }).ToList();

        return new CartResponse
        {
            Id = cart.Id,
            Items = items,
            TotalQuantity = items.Sum(x => x.Quantity),
            TotalAmount = items.Sum(x => x.LineTotal),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
