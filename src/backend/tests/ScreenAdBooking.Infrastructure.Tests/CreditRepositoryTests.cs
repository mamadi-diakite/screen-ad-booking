namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class CreditRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly Company _company = new("Lumen Bakery", null, Now);
    private readonly Company _other = new("Urban Cycle Shop", null, Now);

    public CreditRepositoryTests()
    {
        using var db = _database.CreateContext();
        db.Companies.AddRange(_company, _other);
        db.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Saves_movements_and_lists_a_company_newest_first()
    {
        await using (var db = _database.CreateContext())
        {
            var repository = new CreditRepository(db);
            repository.Add(new CreditMovement(_company.Id, CreditMovementType.TopUp, 500, "Starter pack top-up", Now));
            repository.Add(new CreditMovement(_company.Id, CreditMovementType.Hold, 120, "Booking", Now.AddHours(1)));
            repository.Add(new CreditMovement(_other.Id, CreditMovementType.TopUp, 2000, "Standard pack top-up", Now));
            await repository.SaveChangesAsync(default);
        }

        await using var check = _database.CreateContext();
        var movements = await new CreditRepository(check).ListAsync(_company.Id, default);

        Assert.Equal([CreditMovementType.Hold, CreditMovementType.TopUp], movements.Select(m => m.Type));
        Assert.Equal(Now.AddHours(1), movements[0].CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, movements[0].CreatedAtUtc.Kind);
        Assert.Empty(await new CreditRepository(check).ListAsync(Guid.NewGuid(), default));
    }
}
