using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationCore.Entities;

namespace Infrastructure.Data.Configs;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id); // Primary Key
        //builder.Property(x => x.Id)
        //    .ValueGeneratedOnAdd(); // SQL Server tự tăng Id, tại do tôi sài Guid khi new Product được tạo rồi nên ko cần tự tăng nữa
        builder.Property(x => x.Email)
            .HasMaxLength(200) // Email tối đa 200 ký tự
            .IsRequired(); //Email NOT NULL
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(200);

        builder.HasIndex(x => x.GoogleId).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId);
    }
}