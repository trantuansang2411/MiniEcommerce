using ApplicationCore.Interfaces.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Infrastructure.Payments;

namespace PublicApi.Controllers.Payments;

[ApiController]
[AllowAnonymous]
[Route("api/payments/vnpay")]
public sealed class VnPayCallbackController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly IVnPayGateway _vnPay;
    private readonly IPaymentRepository _paymentRepository;
    private readonly VnPayOptions _options;

    public VnPayCallbackController(IPaymentService payments, IVnPayGateway vnPay, IPaymentRepository paymentRepository, IOptions<VnPayOptions> options)
    {
        _payments = payments;
        _vnPay = vnPay;
        _paymentRepository = paymentRepository;
        _options = options.Value;
    }

    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn()
    {
        var parameters = Request.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString(), StringComparer.Ordinal);
        var response = await _payments.ProcessVnPayIpnAsync(parameters);
        return Ok(new { RspCode = response.RspCode, Message = response.Message });
    }

    [HttpGet("return")]
    public async Task<IActionResult> Return()
    {
        var parameters = Request.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString(), StringComparer.Ordinal);
        var paymentId = parameters.TryGetValue("vnp_TxnRef", out var reference) && Guid.TryParseExact(reference, "N", out var parsedId) ? parsedId : Guid.Empty;
        var valid = _vnPay.IsValidSignature(parameters);
        var payment = valid && paymentId != Guid.Empty ? await _paymentRepository.GetWithOrderAsync(paymentId) : null;
        var orderPath = payment is null ? "/orders" : $"/orders/{payment.OrderId}/payment";
        var query = $"vnpayPaymentId={Uri.EscapeDataString(paymentId.ToString("N"))}&verified={valid.ToString().ToLowerInvariant()}";
        return Redirect($"{_options.FrontendUrl.TrimEnd('/')}{orderPath}?{query}");
    }
}
