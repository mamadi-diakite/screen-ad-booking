using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(Company.NameMaxLength).IsRequired();
        builder.Property(c => c.Sector).HasMaxLength(Company.SectorMaxLength);
    }
}

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(Country.CodeLength).IsFixedLength().IsRequired();
        builder.Property(c => c.Name).HasMaxLength(Country.NameMaxLength).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
    }
}

internal sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(Region.NameMaxLength).IsRequired();
        builder.HasIndex(r => new { r.CountryId, r.Name }).IsUnique();

        builder.HasOne(r => r.Country)
            .WithMany()
            .HasForeignKey(r => r.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(City.NameMaxLength).IsRequired();
        builder.HasIndex(c => new { c.RegionId, c.Name }).IsUnique();

        builder.HasOne(c => c.Region)
            .WithMany()
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).HasMaxLength(Venue.NameMaxLength).IsRequired();
        builder.Property(v => v.Address).HasMaxLength(Venue.AddressMaxLength).IsRequired();
        builder.Property(v => v.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(v => v.Footfall).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.LocationType).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(v => v.CityId);
        builder.HasIndex(v => v.Type);

        builder.HasOne(v => v.City)
            .WithMany()
            .HasForeignKey(v => v.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
