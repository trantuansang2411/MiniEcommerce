using ApplicationCore.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.RecipientName).HasMaxLength(100);
        builder.Property(x => x.RecipientPhone).HasMaxLength(30);
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);
        builder.Property(x => x.DeliveryNote).HasMaxLength(500);

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
