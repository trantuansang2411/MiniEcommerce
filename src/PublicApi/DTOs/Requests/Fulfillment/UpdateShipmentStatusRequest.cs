using ApplicationCore.Entities.Fulfillment; using System.ComponentModel.DataAnnotations;
namespace PublicApi.DTOs.Requests.Fulfillment;
public sealed class UpdateShipmentStatusRequest { [EnumDataType(typeof(ShipmentStatus))] public ShipmentStatus Status {get;init;} [StringLength(100)] public string? ShippingProvider {get;init;} [StringLength(200)] public string? TrackingNumber {get;init;} }
