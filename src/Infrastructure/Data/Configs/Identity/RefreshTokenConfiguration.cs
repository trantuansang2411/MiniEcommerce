using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasOne(x=> x.User).WithMany(x=>x.RefreshTokens).HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.ReplacedByToken).WithOne().HasForeignKey<RefreshToken>(x => x.ReplacedByTokenId).OnDelete(DeleteBehavior.Restrict);
    }
}

