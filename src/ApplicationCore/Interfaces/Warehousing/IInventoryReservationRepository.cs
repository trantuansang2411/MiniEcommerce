using ApplicationCore.Entities.Warehousing;

namespace ApplicationCore.Interfaces.Warehousing;

public interface IInventoryReservationRepository : IRepository<InventoryReservation>
{
    Task<IReadOnlyList<InventoryReservation>> GetActiveByOrderIdAsync(Guid orderId);
}
