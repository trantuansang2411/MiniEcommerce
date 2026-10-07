using System.Security.Claims;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Warehousing;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Warehousing;

namespace PublicApi.Controllers.Warehousing;

[ApiController]
[Route("api/inventories")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? warehouseId)
    {
        var inventories = await _inventoryService.GetAllAsync(warehouseId);
        return Ok(Success(inventories.Select(ToResponse).ToList(), "Inventories retrieved successfully."));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{inventoryId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid inventoryId)
    {
        var transactions = await _inventoryService.GetTransactionsAsync(inventoryId);
        return Ok(Success(transactions.Select(ToResponse).ToList(), "Inventory transactions retrieved successfully."));
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("stock-in")]
    public async Task<IActionResult> StockIn(StockInRequest request)
    {
        var inventory = await _inventoryService.StockInAsync(GetUserId(), request.WarehouseId, request.ProductId, request.Quantity);
        return StatusCode(StatusCodes.Status201Created,
            Success(ToResponse(inventory), "Stock added successfully.", StatusCodes.Status201Created));
    }

    [Authorize(Roles = "Manager")]
    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(AdjustInventoryRequest request)
    {
        var inventory = await _inventoryService.AdjustAsync(GetUserId(), request.WarehouseId, request.ProductId, request.QuantityChange, request.Reason);
        return Ok(Success(ToResponse(inventory), "Inventory adjusted successfully."));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("User is not authenticated.");

        return userId;
    }

    private static ApiResponse<T> Success<T>(T data, string message, int statusCode = StatusCodes.Status200OK) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private static InventoryResponse ToResponse(Inventory inventory) => new()
    {
        Id = inventory.Id,
        WarehouseId = inventory.WarehouseId,
        WarehouseCode = inventory.Warehouse?.Code ?? string.Empty,
        ProductId = inventory.ProductId,
        ProductName = inventory.Product?.Name ?? string.Empty,
        OnHand = inventory.OnHand,
        Reserved = inventory.Reserved,
        Available = inventory.OnHand - inventory.Reserved,
        UpdatedAt = inventory.UpdatedAt
    };

    private static InventoryTransactionResponse ToResponse(InventoryTransaction transaction) => new()
    {
        Id = transaction.Id,
        InventoryId = transaction.InventoryId,
        Type = transaction.Type.ToString(),
        QuantityChange = transaction.QuantityChange,
        Reason = transaction.Reason?.ToString(),
        CreatedByUserId = transaction.CreatedByUserId,
        CreatedAt = transaction.CreatedAt
    };
}
