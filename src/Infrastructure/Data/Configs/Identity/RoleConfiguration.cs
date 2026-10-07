using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly Guid UserRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StaffRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ManagerRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new { Id = UserRoleId, Name = "User" },
            new { Id = AdminRoleId, Name = "Admin" },
            new { Id = StaffRoleId, Name = "Staff" },
            new { Id = ManagerRoleId, Name = "Manager" });
    }
}
