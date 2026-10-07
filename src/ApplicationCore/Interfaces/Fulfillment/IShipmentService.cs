using ApplicationCore.Entities.Fulfillment;
namespace ApplicationCore.Interfaces.Fulfillment;
public interface IShipmentService
{
    Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentStatus? status);
    Task<Shipment> GetDetailAsync(Guid shipmentId);
    Task<IReadOnlyList<Shipment>> GetMyOrderShipmentsAsync(Guid userId, Guid orderId);
    Task<Shipment> UpdateStatusAsync(Guid staffId, Guid shipmentId, ShipmentStatus status, string? provider, string? tracking);
}
