namespace ScreenAdBooking.Domain;

public sealed class City
{
    public const int NameMaxLength = 100;

    private City()
    {
    }

    public City(Guid regionId, string name, double latitude, double longitude)
    {
        Id = Guid.CreateVersion7();
        RegionId = Guard.NotEmpty(regionId, nameof(regionId));
        Name = Guard.Required(name, nameof(name), NameMaxLength);
        Latitude = Coordinates.Latitude(latitude, nameof(latitude));
        Longitude = Coordinates.Longitude(longitude, nameof(longitude));
    }

    public Guid Id { get; private set; }

    public Guid RegionId { get; private set; }

    public Region? Region { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Centre of the city, used to centre the map.</summary>
    public double Latitude { get; private set; }

    public double Longitude { get; private set; }
}
