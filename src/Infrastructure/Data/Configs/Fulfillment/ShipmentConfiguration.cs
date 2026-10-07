using ApplicationCore.Entities.Fulfillment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs.Fulfillment;
public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
 public void Configure(EntityTypeBuilder<Shipment> b) { b.ToTable("Shipments"); b.HasKey(x=>x.Id); b.HasIndex(x=>x.OrderId); b.HasIndex(x=>x.WarehouseId); b.Property(x=>x.ShippingProvider).HasMaxLength(100); b.Property(x=>x.TrackingNumber).HasMaxLength(200); b.HasOne(x=>x.Order).WithMany(x=>x.Shipments).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict); b.HasOne<ApplicationCore.Entities.Warehousing.Warehouse>().WithMany().HasForeignKey(x=>x.WarehouseId).OnDelete(DeleteBehavior.Restrict); }
}
