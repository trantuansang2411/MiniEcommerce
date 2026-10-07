using ApplicationCore.Entities.Catalog;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Catalog;
using ApplicationCore.Interfaces.Warehousing;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services.Warehousing;

public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IRepository<InventoryTransaction> transactionRepository,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Inventory>> GetAllAsync(Guid? warehouseId) => _inventoryRepository.GetAllAsync(warehouseId);

    public async Task<IReadOnlyList<InventoryTransaction>> GetTransactionsAsync(Guid inventoryId)
    {
        if (inventoryId == Guid.Empty)
            throw new BadRequestException("InventoryId is required.");

        if (await _inventoryRepository.GetByIdAsync(inventoryId) is null)
            throw new NotFoundException("Inventory not found.");

        return await _inventoryRepository.GetTransactionsAsync(inventoryId);
    }

    public async Task<Inventory> StockInAsync(Guid managerId, Guid warehouseId, Guid productId, int quantity)
    {
        EnsureManagerAndQuantity(managerId, warehouseId, productId, quantity);
        await EnsureWarehouseAndProductAsync(warehouseId, productId);

        var inventory = await _inventoryRepository.GetByWarehouseAndProductAsync(warehouseId, productId);
        if (inventory is null)
        {
            inventory = new Inventory(warehouseId, productId);
            _inventoryRepository.Add(inventory);
        }

        inventory.StockIn(quantity);
        _transactionRepository.Add(new InventoryTransaction(
            inventory.Id,
            InventoryTransactionType.StockIn,
            quantity,
            managerId));

        await _inventoryRepository.SaveChangesAsync();
        _logger.LogInformation("Manager {ManagerId} stocked in {Quantity} of product {ProductId} at warehouse {WarehouseId}", managerId, quantity, productId, warehouseId);
        return await GetInventoryForResponseAsync(warehouseId, productId);
    }

    public async Task<Inventory> AdjustAsync(Guid managerId, Guid warehouseId, Guid productId, int quantityChange, InventoryAdjustmentReason reason)
    {
        if (quantityChange == 0)
            throw new BadRequestException("Adjustment quantity cannot be zero.");

        EnsureManagerAndIds(managerId, warehouseId, productId);
        await EnsureWarehouseAndProductAsync(warehouseId, productId);

        var inventory = await _inventoryRepository.GetByWarehouseAndProductAsync(warehouseId, productId)
            ?? throw new NotFoundException("Inventory not found for this warehouse and product.");

        try
        {
            inventory.Adjust(quantityChange);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        _transactionRepository.Add(new InventoryTransaction(
            inventory.Id,
            InventoryTransactionType.Adjustment,
            quantityChange,
            managerId,
            reason));

        await _inventoryRepository.SaveChangesAsync();
        _logger.LogInformation("Manager {ManagerId} adjusted inventory {InventoryId} by {QuantityChange}", managerId, inventory.Id, quantityChange);
        return await GetInventoryForResponseAsync(warehouseId, productId);
    }

    private async Task EnsureWarehouseAndProductAsync(Guid warehouseId, Guid productId)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId)
            ?? throw new NotFoundException("Warehouse not found.");

        if (warehouse.Status != WarehouseStatus.Active)
            throw new ConflictException("Warehouse is not active.");

        if (await _productRepository.GetByIdAsync(productId) is null)
            throw new NotFoundException("Product not found.");
    }

    private static void EnsureManagerAndQuantity(Guid managerId, Guid warehouseId, Guid productId, int quantity)
    {
        EnsureManagerAndIds(managerId, warehouseId, productId);
        if (quantity <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");
    }

    private static void EnsureManagerAndIds(Guid managerId, Guid warehouseId, Guid productId)
    {
        if (managerId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");

        if (warehouseId == Guid.Empty)
            throw new BadRequestException("WarehouseId is required.");

        if (productId == Guid.Empty)
            throw new BadRequestException("ProductId is required.");
    }

    private async Task<Inventory> GetInventoryForResponseAsync(Guid warehouseId, Guid productId)
    {
        return await _inventoryRepository.GetByWarehouseAndProductForReadAsync(warehouseId, productId)
            ?? throw new InvalidOperationException("Inventory could not be reloaded after saving.");
    }
}
