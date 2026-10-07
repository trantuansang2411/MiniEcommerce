using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;
public class OtpVerificationConfiguration: IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("OtpVerifications");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OtpHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(x => x.User).WithMany(x => x.OtpVerifications).HasForeignKey(x => x.UserId);
    }
}

