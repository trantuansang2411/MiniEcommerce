using ApplicationCore.Entities.Fulfillment;
using ApplicationCore.Interfaces;
namespace ApplicationCore.Interfaces.Fulfillment;
public interface IShipmentRepository : IRepository<Shipment>
{
    Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentStatus? status);
    Task<Shipment?> GetDetailAsync(Guid id);
    Task<IReadOnlyList<Shipment>> GetByOrderForUserAsync(Guid orderId, Guid userId);
    Task<Shipment?> GetForUpdateAsync(Guid id);
    Task<bool> HasOtherUndeliveredAsync(Guid orderId, Guid shipmentId);
}
