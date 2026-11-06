using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Domain;
using ScreenAdBooking.Infrastructure.Persistence;

namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class UserRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly User _user = new("nora.vidal@example.com", "Nora", "hash", UserRole.AgencyAdmin, null, Now);

    public UserRepositoryTests()
    {
        using var db = _database.CreateContext();
        db.Users.Add(_user);
        db.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Finds_a_user_by_email_and_by_id()
    {
        await using var db = _database.CreateContext();
        var repository = new UserRepository(db);

        Assert.Equal(_user.Id, (await repository.FindByEmailAsync("nora.vidal@example.com", default))?.Id);
        Assert.Equal(_user.Email, (await repository.FindByIdAsync(_user.Id, default))?.Email);
        Assert.Null(await repository.FindByEmailAsync("nobody@example.com", default));
        Assert.Null(await repository.FindByIdAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task Adds_an_advertiser_with_its_company_and_saves_a_changed_password()
    {
        var company = new Company("Lumen Bakery", null, Now);
        var advertiser = new User("paul@example.com", "Paul", "hash", UserRole.Advertiser, company.Id, Now);
        await using (var db = _database.CreateContext())
        {
            var repository = new UserRepository(db);
            repository.AddCompany(company);
            repository.Add(advertiser);
            (await repository.FindByIdAsync(_user.Id, default))!.ChangePassword("new-hash");
            await repository.SaveChangesAsync(default);
        }

        await using var check = _database.CreateContext();
        var saved = await check.Users.Include(u => u.Company).SingleAsync(u => u.Id == advertiser.Id);
        Assert.Equal("Lumen Bakery", saved.Company?.Name);
        Assert.Equal("new-hash", (await new UserRepository(check).FindByIdAsync(_user.Id, default))?.PasswordHash);
    }
}
