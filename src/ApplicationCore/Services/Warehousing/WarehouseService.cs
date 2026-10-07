using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Warehousing;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(IWarehouseRepository warehouseRepository, ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Warehouse>> GetAllAsync() => _warehouseRepository.GetAllAsync();

    public async Task<Warehouse> GetByIdAsync(Guid id)
    {
        return await _warehouseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Warehouse not found.");
    }

    public async Task<Warehouse> CreateAsync(string code, string name, string address)
    {
        var warehouse = new Warehouse(code, name, address);
        if (await _warehouseRepository.GetByCodeAsync(warehouse.Code) is not null)
            throw new ConflictException("Warehouse code already exists.");

        _warehouseRepository.Add(warehouse);
        await _warehouseRepository.SaveChangesAsync();
        _logger.LogInformation("Warehouse {WarehouseId} created with code {WarehouseCode}", warehouse.Id, warehouse.Code);
        return warehouse;
    }

    public async Task<Warehouse> UpdateAsync(Guid id, string? name, string? address, WarehouseStatus? status)
    {
        if (name is null && address is null && !status.HasValue)
            throw new BadRequestException("At least one field must be provided for update.");

        if (name is not null && string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Warehouse name cannot be empty.");

        if (address is not null && string.IsNullOrWhiteSpace(address))
            throw new BadRequestException("Warehouse address cannot be empty.");

        var warehouse = await GetByIdAsync(id);
        warehouse.Update(name, address, status);
        await _warehouseRepository.SaveChangesAsync();
        _logger.LogInformation("Warehouse {WarehouseId} updated", warehouse.Id);
        return warehouse;
    }
}
