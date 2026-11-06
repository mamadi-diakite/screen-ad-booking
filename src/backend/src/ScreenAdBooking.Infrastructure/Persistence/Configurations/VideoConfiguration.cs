namespace ScreenAdBooking.Infrastructure.Persistence.Configurations;

internal sealed class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).HasMaxLength(Video.NameMaxLength).IsRequired();
        builder.Property(v => v.Format).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.RejectionReason).HasMaxLength(Video.RejectionReasonMaxLength);
        builder.HasIndex(v => new { v.CompanyId, v.UploadedAtUtc });

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
