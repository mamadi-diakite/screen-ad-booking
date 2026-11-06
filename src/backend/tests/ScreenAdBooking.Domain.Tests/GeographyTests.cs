namespace ScreenAdBooking.Domain.Tests;

public sealed class GeographyTests
{
    [Fact]
    public void Creates_a_country_with_an_uppercase_code()
    {
        var country = new Country(" fr ", " France ");

        Assert.NotEqual(Guid.Empty, country.Id);
        Assert.Equal("FR", country.Code);
        Assert.Equal("France", country.Name);
    }

    [Theory]
    [InlineData("F")]
    [InlineData("F1")]
    [InlineData("FRA")]
    [InlineData(" ")]
    public void Rejects_invalid_country_codes(string code)
    {
        var error = Assert.Throws<ArgumentException>(() => new Country(code, "France"));

        Assert.Equal("code", error.ParamName);
    }

    [Fact]
    public void Country_needs_a_name()
    {
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => new Country("FR", " ")).ParamName);
    }

    [Fact]
    public void Creates_a_region_in_a_country()
    {
        var countryId = Guid.NewGuid();

        var region = new Region(countryId, " Brittany ");

        Assert.NotEqual(Guid.Empty, region.Id);
        Assert.Equal(countryId, region.CountryId);
        Assert.Equal("Brittany", region.Name);
    }

    [Fact]
    public void Region_needs_a_country_and_a_name()
    {
        Assert.Equal("countryId", Assert.Throws<ArgumentException>(() => new Region(Guid.Empty, "Brittany")).ParamName);
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => new Region(Guid.NewGuid(), " ")).ParamName);
    }

    [Fact]
    public void Creates_a_city_with_coordinates()
    {
        var regionId = Guid.NewGuid();

        var city = new City(regionId, " Rennes ", 48.1173, -1.6778);

        Assert.NotEqual(Guid.Empty, city.Id);
        Assert.Equal(regionId, city.RegionId);
        Assert.Equal("Rennes", city.Name);
        Assert.Equal(48.1173, city.Latitude);
        Assert.Equal(-1.6778, city.Longitude);
    }

    [Fact]
    public void City_needs_a_region_and_a_name()
    {
        Assert.Equal("regionId", Assert.Throws<ArgumentException>(() => new City(Guid.Empty, "Rennes", 0, 0)).ParamName);
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => new City(Guid.NewGuid(), " ", 0, 0)).ParamName);
    }

    [Theory]
    [InlineData(-90.0, -180.0)]
    [InlineData(90.0, 180.0)]
    public void Accepts_coordinates_at_the_limits(double latitude, double longitude)
    {
        var city = new City(Guid.NewGuid(), "Edge", latitude, longitude);

        Assert.Equal(latitude, city.Latitude);
        Assert.Equal(longitude, city.Longitude);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Rejects_invalid_latitude(double latitude)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new City(Guid.NewGuid(), "Rennes", latitude, 0));

        Assert.Equal("latitude", error.ParamName);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void Rejects_invalid_longitude(double longitude)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new City(Guid.NewGuid(), "Rennes", 0, longitude));

        Assert.Equal("longitude", error.ParamName);
    }
}
