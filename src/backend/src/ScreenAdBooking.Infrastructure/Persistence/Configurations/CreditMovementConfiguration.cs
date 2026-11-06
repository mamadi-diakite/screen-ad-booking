using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Persistence.Configurations;

internal sealed class CreditMovementConfiguration : IEntityTypeConfiguration<CreditMovement>
{
    public void Configure(EntityTypeBuilder<CreditMovement> builder)
    {
        builder.ToTable("CreditMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Label).HasMaxLength(CreditMovement.LabelMaxLength).IsRequired();
        builder.HasIndex(m => new { m.CompanyId, m.CreatedAtUtc });

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(m => m.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
