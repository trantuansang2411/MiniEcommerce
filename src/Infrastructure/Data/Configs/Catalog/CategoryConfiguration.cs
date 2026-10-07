using ApplicationCore.Entities;
using ApplicationCore.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs.Catalog;

public class CategoryConfiguration : IEntityTypeConfiguration<Category> 
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(c => c.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.IconKey)
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new { Id = new Guid("00000000-0000-0000-0000-000000000101"), Name = "Laptop", Description = "Laptop phục vụ học tập, làm việc và giải trí.", IconKey = "laptop", IsActive = true, CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000102"), Name = "Điện thoại", Description = "Điện thoại thông minh chính hãng.", IconKey = "smartphone", IsActive = true, CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000103"), Name = "Tablet", Description = "Máy tính bảng linh hoạt cho công việc và giải trí.", IconKey = "tablet", IsActive = true, CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000104"), Name = "Tai nghe", Description = "Tai nghe không dây và có dây chất lượng cao.", IconKey = "headphones", IsActive = true, CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new { Id = new Guid("00000000-0000-0000-0000-000000000105"), Name = "Thiết bị thông minh", Description = "Thiết bị công nghệ thông minh cho cuộc sống hiện đại.", IconKey = "watch", IsActive = true, CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) });
    }
}
