using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Requests.Warehousing;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Warehousing;

namespace PublicApi.Controllers.Warehousing;

[ApiController]
[Route("api/warehouses")]
public sealed class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseService.GetAllAsync();
        return Ok(Success(warehouses.Select(ToResponse).ToList(), "Warehouses retrieved successfully."));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        return Ok(Success(ToResponse(warehouse), "Warehouse retrieved successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateWarehouseRequest request)
    {
        var warehouse = await _warehouseService.CreateAsync(request.Code, request.Name, request.Address);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id },
            Success(ToResponse(warehouse), "Warehouse created successfully.", StatusCodes.Status201Created));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateWarehouseRequest request)
    {
        var warehouse = await _warehouseService.UpdateAsync(id, request.Name, request.Address, request.Status);
        return Ok(Success(ToResponse(warehouse), "Warehouse updated successfully."));
    }

    private static ApiResponse<T> Success<T>(T data, string message, int statusCode = StatusCodes.Status200OK) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private static WarehouseResponse ToResponse(Warehouse warehouse) => new()
    {
        Id = warehouse.Id,
        Code = warehouse.Code,
        Name = warehouse.Name,
        Address = warehouse.Address,
        Status = warehouse.Status.ToString(),
        CreatedAt = warehouse.CreatedAt,
        UpdatedAt = warehouse.UpdatedAt
    };
}
