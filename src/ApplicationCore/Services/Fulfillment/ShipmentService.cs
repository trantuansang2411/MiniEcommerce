using ApplicationCore.Entities.Fulfillment;
using ApplicationCore.Entities.Warehousing;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Fulfillment;
using ApplicationCore.Interfaces.Warehousing;

namespace ApplicationCore.Services.Fulfillment;

public sealed class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipments;
    private readonly IInventoryReservationRepository _reservations;
    private readonly IRepository<InventoryTransaction> _transactions;

    public ShipmentService(
        IShipmentRepository shipments,
        IInventoryReservationRepository reservations,
        IRepository<InventoryTransaction> transactions)
    {
        _shipments = shipments;
        _reservations = reservations;
        _transactions = transactions;
    }

    public Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentStatus? status) =>
        _shipments.GetAllAsync(status);

    public async Task<Shipment> GetDetailAsync(Guid shipmentId)
    {
        return await _shipments.GetDetailAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");
    }

    public async Task<IReadOnlyList<Shipment>> GetMyOrderShipmentsAsync(Guid userId, Guid orderId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("User is not authenticated.");

        return await _shipments.GetByOrderForUserAsync(orderId, userId);
    }

    public async Task<Shipment> UpdateStatusAsync(
        Guid staffId,
        Guid shipmentId,
        ShipmentStatus status,
        string? provider,
        string? tracking)
    {
        var shipment = await _shipments.GetForUpdateAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        try
        {
            shipment.MoveTo(status, provider, tracking);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        if (status == ShipmentStatus.Shipped)
            await ConsumeReservationsAsync(shipment, staffId);

        if (status == ShipmentStatus.Delivered)
        {
            var hasOtherUndelivered = await _shipments.HasOtherUndeliveredAsync(
                shipment.OrderId,
                shipment.Id);

            if (!hasOtherUndelivered)
            {
                try
                {
                    shipment.Order.MarkAsCompleted();
                }
                catch (InvalidOperationException exception)
                {
                    throw new ConflictException(exception.Message);
                }
            }
        }

        await _shipments.SaveChangesAsync();
        return shipment;
    }

    private async Task ConsumeReservationsAsync(Shipment shipment, Guid staffId)
    {
        var reservations = await _reservations.GetActiveByOrderIdAsync(shipment.OrderId);

        foreach (var item in shipment.Items)
        {
            var reservation = reservations.SingleOrDefault(x => x.OrderItemId == item.OrderItemId)
                ?? throw new ConflictException("Active reservation not found.");

            try
            {
                reservation.Inventory.ConsumeReservation(item.Quantity);
                reservation.Consume();
            }
            catch (InvalidOperationException exception)
            {
                throw new ConflictException(exception.Message);
            }

            _transactions.Add(new InventoryTransaction(
                reservation.InventoryId,
                InventoryTransactionType.StockOut,
                -item.Quantity,
                staffId));
        }
    }
}
