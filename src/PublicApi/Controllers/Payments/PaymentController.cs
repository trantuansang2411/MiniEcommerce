using System.Security.Claims;
using ApplicationCore.Entities.Payments;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.DTOs.Responses;
using PublicApi.DTOs.Responses.Payments;

namespace PublicApi.Controllers.Payments;

[ApiController]
[Authorize]
[Route("api/orders/{orderId:guid}/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("manual")]
    public async Task<IActionResult> PayManually(Guid orderId)
    {
        var payment = await _paymentService.PayOrderManuallyAsync(GetUserId(), orderId);

        return StatusCode(StatusCodes.Status201Created, new ApiResponse<PaymentResponse>
        {
            StatusCode = StatusCodes.Status201Created,
            Message = "Payment completed successfully.",
            Data = ToResponse(payment)
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("User is not authenticated.");

        return userId;
    }

    private static PaymentResponse ToResponse(Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        Amount = payment.Amount,
        Method = payment.Method.ToString(),
        Status = payment.Status.ToString(),
        TransactionId = payment.TransactionId,
        CreatedAt = payment.CreatedAt,
        PaidAt = payment.PaidAt
    };
}
