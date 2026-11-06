namespace ScreenAdBooking.Domain.Tests;

public sealed class VenueTests
{
    private static readonly Guid CityId = Guid.NewGuid();

    [Fact]
    public void Creates_a_venue()
    {
        var venue = new Venue(
            CityId, " Fresh Market 12 ", VenueType.Supermarket, Footfall.High, LocationType.CityCentre,
            " 4 Example Square ", 48.111, -1.68, 3, 40, TestDates.Now);

        Assert.NotEqual(Guid.Empty, venue.Id);
        Assert.Equal(CityId, venue.CityId);
        Assert.Equal("Fresh Market 12", venue.Name);
        Assert.Equal(VenueType.Supermarket, venue.Type);
        Assert.Equal(Footfall.High, venue.Footfall);
        Assert.Equal(LocationType.CityCentre, venue.LocationType);
        Assert.Equal("4 Example Square", venue.Address);
        Assert.Equal(48.111, venue.Latitude);
        Assert.Equal(-1.68, venue.Longitude);
        Assert.Equal(3, venue.ScreenCount);
        Assert.Equal(40, venue.AverageAudience);
        Assert.Equal(TestDates.Now, venue.CreatedAtUtc);
    }

    [Fact]
    public void Average_audience_can_be_zero()
    {
        Assert.Equal(0, NewVenue(averageAudience: 0).AverageAudience);
    }

    [Fact]
    public void Rejects_invalid_values()
    {
        Assert.Equal("cityId", Assert.Throws<ArgumentException>(() => NewVenue(cityId: Guid.Empty)).ParamName);
        Assert.Equal("name", Assert.Throws<ArgumentException>(() => NewVenue(name: " ")).ParamName);
        Assert.Equal("type", Assert.Throws<ArgumentOutOfRangeException>(() => NewVenue(type: (VenueType)99)).ParamName);
        Assert.Equal("footfall", Assert.Throws<ArgumentOutOfRangeException>(() => NewVenue(footfall: (Footfall)9)).ParamName);
        Assert.Equal("locationType", Assert.Throws<ArgumentOutOfRangeException>(
            () => NewVenue(locationType: (LocationType)9)).ParamName);
        Assert.Equal("address", Assert.Throws<ArgumentException>(() => NewVenue(address: " ")).ParamName);
        Assert.Equal("latitude", Assert.Throws<ArgumentOutOfRangeException>(() => NewVenue(latitude: 91)).ParamName);
        Assert.Equal("longitude", Assert.Throws<ArgumentOutOfRangeException>(() => NewVenue(longitude: 181)).ParamName);
        Assert.Equal("screenCount", Assert.Throws<ArgumentOutOfRangeException>(() => NewVenue(screenCount: 0)).ParamName);
        Assert.Equal("averageAudience", Assert.Throws<ArgumentOutOfRangeException>(
            () => NewVenue(averageAudience: -1)).ParamName);
        Assert.Equal("createdAtUtc", Assert.Throws<ArgumentException>(
            () => NewVenue(createdAtUtc: DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }

    private static Venue NewVenue(
        Guid? cityId = null,
        string name = "Fresh Market 12",
        VenueType type = VenueType.Supermarket,
        Footfall footfall = Footfall.Medium,
        LocationType locationType = LocationType.Suburb,
        string address = "4 Example Square",
        double latitude = 48.1,
        double longitude = -1.6,
        int screenCount = 1,
        int averageAudience = 25,
        DateTime? createdAtUtc = null) =>
        new(cityId ?? CityId, name, type, footfall, locationType, address, latitude, longitude,
            screenCount, averageAudience, createdAtUtc ?? TestDates.Now);
}
