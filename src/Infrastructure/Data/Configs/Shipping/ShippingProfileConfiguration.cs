using ApplicationCore.Entities.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs.Shipping;

public sealed class ShippingProfileConfiguration : IEntityTypeConfiguration<ShippingProfile>
{
    public void Configure(EntityTypeBuilder<ShippingProfile> builder)
    {
        builder.ToTable("ShippingProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.RecipientName).HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.RecipientPhone).HasMaxLength(30).IsRequired();
        builder.Property(profile => profile.ShippingAddress).HasMaxLength(500).IsRequired();
        builder.HasIndex(profile => new { profile.UserId, profile.IsDefault });
        builder.HasOne(profile => profile.User)
            .WithMany(user => user.ShippingProfiles)
            .HasForeignKey(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
