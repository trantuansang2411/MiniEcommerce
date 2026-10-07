using System.Security.Claims;
using ApplicationCore.Entities.Orders;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Orders;
using ApplicationCore.Interfaces.Fulfillment;
using PublicApi.DTOs.Responses.Fulfillment;
using PublicApi.Controllers.Fulfillment;
using PublicApi.DTOs.Requests.Orders;
using ApplicationCore.Services.Orders;

namespace PublicApi.Controllers.Orders;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IShipmentService _shipmentService;

    public OrderController(IOrderService orderService, IShipmentService shipmentService)
    {
        _orderService = orderService;
        _shipmentService = shipmentService;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        var order = await _orderService.CheckoutAsync(GetUserId(), new CheckoutShippingInfo(
            request.ShippingProfileId,
            request.RecipientName,
            request.RecipientPhone,
            request.ShippingAddress,
            request.DeliveryNote,
            request.SaveAsProfile,
            request.SetAsDefault));

        return StatusCode(StatusCodes.Status201Created, new ApiResponse<OrderResponse>
        {
            StatusCode = StatusCodes.Status201Created,
            Message = "Order created successfully. Please complete payment before it expires.",
            Data = ToResponse(order)
        });
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _orderService.GetMyOrdersAsync(GetUserId());

        return Ok(new ApiResponse<IReadOnlyList<OrderResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Orders retrieved successfully.",
            Data = orders.Select(ToResponse).ToList()
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("management")]
    public async Task<IActionResult> GetAllForManagement([FromQuery] OrderStatus? status)
    {
        var orders = await _orderService.GetAllForManagementAsync(status);

        return Ok(new ApiResponse<IReadOnlyList<OrderResponse>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Orders retrieved successfully.",
            Data = orders.Select(ToResponse).ToList()
        });
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("management/{orderId:guid}")]
    public async Task<IActionResult> GetManagementDetail(Guid orderId)
    {
        var order = await _orderService.GetManagementDetailAsync(orderId);
        return Ok(new ApiResponse<OrderManagementDetailResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Order retrieved successfully.",
            Data = ToManagementDetailResponse(order)
        });
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetMyOrderById(Guid orderId)
    {
        var order = await _orderService.GetMyOrderByIdAsync(GetUserId(), orderId);

        return Ok(new ApiResponse<OrderResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Order retrieved successfully.",
            Data = ToResponse(order)
        });
    }

    [HttpGet("{orderId:guid}/shipments")]
    public async Task<IActionResult> GetMyShipments(Guid orderId)
    {
        var shipments = await _shipmentService.GetMyOrderShipmentsAsync(GetUserId(), orderId);
        return Ok(new ApiResponse<IReadOnlyList<ShipmentResponse>> { StatusCode = 200, Message = "Shipments retrieved successfully.", Data = shipments.Select(ShipmentController.Map).ToList() });
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        await _orderService.CancelAsync(GetUserId(), orderId);

        return Ok(new ApiResponse<object?>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Order cancelled successfully.",
            Data = null
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("User is not authenticated.");

        return userId;
    }

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        TotalAmount = order.TotalAmount,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        ExpiresAt = order.ExpiresAt,
        RecipientName = order.RecipientName,
        RecipientPhone = order.RecipientPhone,
        ShippingAddress = order.ShippingAddress,
        DeliveryNote = order.DeliveryNote,
        Items = order.Items.Select(item => new OrderItemResponse
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.UnitPrice * item.Quantity
        }).ToList()
    };

    private static OrderManagementDetailResponse ToManagementDetailResponse(Order order) => new()
    {
        Id = order.Id,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        ExpiresAt = order.ExpiresAt,
        Delivery = new OrderDeliveryResponse { RecipientName = order.RecipientName, RecipientPhone = order.RecipientPhone, ShippingAddress = order.ShippingAddress, DeliveryNote = order.DeliveryNote },
        Items = order.Items.Select(item => new OrderManagementItemResponse { ProductId = item.ProductId, ProductName = item.Product.Name, ThumbnailUrl = item.Product.ThumbnailUrl, Quantity = item.Quantity, UnitPrice = item.UnitPrice, LineTotal = item.UnitPrice * item.Quantity }).ToList(),
        Payments = order.Payments.OrderByDescending(payment => payment.CreatedAt).Select(payment => new OrderPaymentResponse { Id = payment.Id, Method = payment.Method.ToString(), Status = payment.Status.ToString(), TransactionId = payment.TransactionId, Amount = payment.Amount, CreatedAt = payment.CreatedAt, PaidAt = payment.PaidAt }).ToList(),
        Shipments = order.Shipments.OrderBy(shipment => shipment.CreatedAt).Select(shipment => new OrderShipmentSummaryResponse { Id = shipment.Id, Status = shipment.Status.ToString(), ShippingProvider = shipment.ShippingProvider, TrackingNumber = shipment.TrackingNumber, ItemCount = shipment.Items.Sum(item => item.Quantity), CreatedAt = shipment.CreatedAt, ShippedAt = shipment.ShippedAt, DeliveredAt = shipment.DeliveredAt }).ToList()
    };
}
