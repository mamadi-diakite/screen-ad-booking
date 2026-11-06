namespace ScreenAdBooking.Domain.Tests;

public sealed class CompanyTests
{
    [Fact]
    public void Creates_a_company()
    {
        var company = new Company(" Fresh Juice Co ", " Food and drinks ", TestDates.Now);

        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal("Fresh Juice Co", company.Name);
        Assert.Equal("Food and drinks", company.Sector);
        Assert.Equal(TestDates.Now, company.CreatedAtUtc);
    }

    [Fact]
    public void Sector_is_optional()
    {
        Assert.Null(new Company("Fresh Juice Co", null, TestDates.Now).Sector);
    }

    [Fact]
    public void Rejects_blank_name_and_local_date()
    {
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => new Company(" ", null, TestDates.Now)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => new Company("Fresh", null, DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }
}
