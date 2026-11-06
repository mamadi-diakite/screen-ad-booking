using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Infrastructure.Persistence;

namespace ScreenAdBooking.Infrastructure.Tests;

/// <summary>In-memory SQLite database that lives as long as its connection stays open.</summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.Migrate();
    }

    public ScreenAdBookingDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ScreenAdBookingDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
