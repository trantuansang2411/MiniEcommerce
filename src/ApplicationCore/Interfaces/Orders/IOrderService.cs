using ApplicationCore.Entities.Orders;
using ApplicationCore.Services.Orders;

namespace ApplicationCore.Interfaces.Orders;

public interface IOrderService
{
    Task<Order> CheckoutAsync(Guid userId, CheckoutShippingInfo shippingInfo);
    Task<IReadOnlyList<Order>> GetMyOrdersAsync(Guid userId);
    Task<Order> GetMyOrderByIdAsync(Guid userId, Guid orderId);
    Task<IReadOnlyList<Order>> GetAllForManagementAsync(OrderStatus? status);
    Task<Order> GetManagementDetailAsync(Guid orderId);
    Task CancelAsync(Guid userId, Guid orderId);
}
