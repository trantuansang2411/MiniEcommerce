using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ApplicationCore.Interfaces.Payments;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

public sealed class VnPayGateway : IVnPayGateway
{
    private readonly VnPayOptions _options;

    public VnPayGateway(IOptions<VnPayOptions> options) => _options = options.Value;

    public string CreatePaymentUrl(VnPayPaymentRequest request)
    {
        EnsureConfigured();
        var localTime = ToVietnamTime(DateTimeOffset.UtcNow);
        var expiry = ToVietnamTime(request.ExpiresAt);
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0", ["vnp_Command"] = "pay", ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = decimal.ToInt64(request.Amount * 100m).ToString(CultureInfo.InvariantCulture), ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = request.PaymentId.ToString("N"), ["vnp_OrderInfo"] = $"Thanh toan don hang {request.OrderId:N}",
            ["vnp_OrderType"] = "other", ["vnp_Locale"] = "vn", ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_IpAddr"] = request.ClientIp, ["vnp_CreateDate"] = localTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = expiry.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };
        var query = ToQueryString(parameters);
        return $"{_options.PaymentUrl}?{query}&vnp_SecureHash={Sign(query)}";
    }

    public bool IsValidSignature(IReadOnlyDictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(_options.HashSecret) || !parameters.TryGetValue("vnp_SecureHash", out var receivedHash) || !parameters.TryGetValue("vnp_TmnCode", out var tmnCode) || !string.Equals(tmnCode, _options.TmnCode, StringComparison.Ordinal)) return false;
        var data = parameters.Where(x => x.Key.StartsWith("vnp_", StringComparison.Ordinal) && x.Key is not "vnp_SecureHash" and not "vnp_SecureHashType").OrderBy(x => x.Key, StringComparer.Ordinal).ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
        var expectedHash = Sign(ToQueryString(data));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedHash.ToUpperInvariant()), Encoding.UTF8.GetBytes(receivedHash.ToUpperInvariant()));
    }

    private string Sign(string data) { using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HashSecret)); return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant(); }
    // VNPay's reference implementations use application/x-www-form-urlencoded encoding.
    // In particular, a space must be encoded as '+' (not '%20') before computing HMAC-SHA512.
    private static string ToQueryString(IEnumerable<KeyValuePair<string, string>> parameters) => string.Join("&", parameters.Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}"));
    private void EnsureConfigured() { if (string.IsNullOrWhiteSpace(_options.TmnCode) || string.IsNullOrWhiteSpace(_options.HashSecret) || string.IsNullOrWhiteSpace(_options.ReturnUrl)) throw new InvalidOperationException("VnPay is not configured. Set VnPay:TmnCode, VnPay:HashSecret, and VnPay:ReturnUrl in secure server configuration."); }
    private static DateTimeOffset ToVietnamTime(DateTimeOffset value) { try { return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, "SE Asia Standard Time"); } catch (TimeZoneNotFoundException) { return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, "Asia/Ho_Chi_Minh"); } }
}
