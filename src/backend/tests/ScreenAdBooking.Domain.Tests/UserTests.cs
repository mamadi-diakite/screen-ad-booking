namespace ScreenAdBooking.Domain.Tests;

public sealed class UserTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();

    [Fact]
    public void Creates_an_advertiser_in_a_company()
    {
        var user = new User("  Anna.Smith@Example.com ", " Anna Smith ", "hash", UserRole.Advertiser, CompanyId, TestDates.Now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("anna.smith@example.com", user.Email);
        Assert.Equal("Anna Smith", user.DisplayName);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(UserRole.Advertiser, user.Role);
        Assert.Equal(CompanyId, user.CompanyId);
        Assert.Equal(TestDates.Now, user.CreatedAtUtc);
    }

    [Fact]
    public void Creates_an_agency_admin_without_company()
    {
        var user = new User("admin@example.com", "Agency Admin", "hash", UserRole.AgencyAdmin, null, TestDates.Now);

        Assert.Equal(UserRole.AgencyAdmin, user.Role);
        Assert.Null(user.CompanyId);
    }

    [Fact]
    public void Advertiser_needs_a_company()
    {
        Assert.Equal("companyId", Assert.Throws<ArgumentException>(
            () => new User("a@example.com", "Anna", "hash", UserRole.Advertiser, null, TestDates.Now)).ParamName);
        Assert.Equal("companyId", Assert.Throws<ArgumentException>(
            () => new User("a@example.com", "Anna", "hash", UserRole.Advertiser, Guid.Empty, TestDates.Now)).ParamName);
    }

    [Fact]
    public void Agency_admin_cannot_belong_to_a_company()
    {
        var error = Assert.Throws<ArgumentException>(
            () => new User("admin@example.com", "Admin", "hash", UserRole.AgencyAdmin, CompanyId, TestDates.Now));

        Assert.Equal("companyId", error.ParamName);
    }

    [Fact]
    public void Rejects_invalid_values()
    {
        Assert.Equal("email", Assert.Throws<ArgumentException>(
            () => new User("anna@", "Anna", "hash", UserRole.Advertiser, CompanyId, TestDates.Now)).ParamName);
        Assert.Equal("displayName", Assert.Throws<ArgumentException>(
            () => new User("a@example.com", " ", "hash", UserRole.Advertiser, CompanyId, TestDates.Now)).ParamName);
        Assert.Equal("passwordHash", Assert.Throws<ArgumentException>(
            () => new User("a@example.com", "Anna", "", UserRole.Advertiser, CompanyId, TestDates.Now)).ParamName);
        Assert.Equal("role", Assert.Throws<ArgumentOutOfRangeException>(
            () => new User("a@example.com", "Anna", "hash", (UserRole)9, CompanyId, TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new User("a@example.com", "Anna", "hash", UserRole.Advertiser, CompanyId,
                DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }
}
