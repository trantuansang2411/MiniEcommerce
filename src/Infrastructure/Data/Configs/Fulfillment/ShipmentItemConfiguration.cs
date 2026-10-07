using ApplicationCore.Entities.Fulfillment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configs.Fulfillment;
public sealed class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{ public void Configure(EntityTypeBuilder<ShipmentItem> b) { b.ToTable("ShipmentItems"); b.HasKey(x=>x.Id); b.HasOne(x=>x.Shipment).WithMany(x=>x.Items).HasForeignKey(x=>x.ShipmentId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.OrderItem).WithMany().HasForeignKey(x=>x.OrderItemId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict); } }
