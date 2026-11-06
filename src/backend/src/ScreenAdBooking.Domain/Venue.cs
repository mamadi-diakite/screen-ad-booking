namespace ScreenAdBooking.Domain;

/// <summary>A place with one or more screens.</summary>
public sealed class Venue
{
    public const int NameMaxLength = 150;
    public const int AddressMaxLength = 200;

    private Venue()
    {
    }

    public Venue(
        Guid cityId,
        string name,
        VenueType type,
        Footfall footfall,
        LocationType locationType,
        string address,
        double latitude,
        double longitude,
        int screenCount,
        int averageAudience,
        DateTime createdAtUtc)
    {
        Id = Guid.CreateVersion7();
        CityId = Guard.NotEmpty(cityId, nameof(cityId));
        Name = Guard.Required(name, nameof(name), NameMaxLength);
        Type = Guard.Defined(type, nameof(type));
        Footfall = Guard.Defined(footfall, nameof(footfall));
        LocationType = Guard.Defined(locationType, nameof(locationType));
        Address = Guard.Required(address, nameof(address), AddressMaxLength);
        Latitude = Coordinates.Latitude(latitude, nameof(latitude));
        Longitude = Coordinates.Longitude(longitude, nameof(longitude));
        ScreenCount = Guard.Positive(screenCount, nameof(screenCount));
        AverageAudience = Guard.NotNegative(averageAudience, nameof(averageAudience));
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid CityId { get; private set; }

    public City? City { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public VenueType Type { get; private set; }

    public Footfall Footfall { get; private set; }

    public LocationType LocationType { get; private set; }

    public string Address { get; private set; } = string.Empty;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public int ScreenCount { get; private set; }

    /// <summary>Average number of people who see one play, set by the agency.</summary>
    public int AverageAudience { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
