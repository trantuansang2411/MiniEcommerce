using ApplicationCore.Entities.Orders;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Interfaces.Orders;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId);
    Task<Order?> GetByIdForUserAsync(Guid orderId, Guid userId);
    Task<Order?> GetForPaymentByIdForUserAsync(Guid orderId, Guid userId);
    Task<Order?> GetForUpdateByIdForUserAsync(Guid orderId, Guid userId);
    Task<IReadOnlyList<Order>> GetAllForManagementAsync(OrderStatus? status);
    Task<Order?> GetManagementDetailAsync(Guid orderId);
}
