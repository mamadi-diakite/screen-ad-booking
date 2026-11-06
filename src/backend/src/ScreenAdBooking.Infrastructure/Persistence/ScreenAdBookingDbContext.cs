using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Persistence;

public sealed class ScreenAdBookingDbContext(DbContextOptions<ScreenAdBookingDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Region> Regions => Set<Region>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Venue> Venues => Set<Venue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScreenAdBookingDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
}
