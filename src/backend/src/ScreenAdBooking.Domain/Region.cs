namespace ScreenAdBooking.Domain;

public sealed class Region
{
    public const int NameMaxLength = 100;

    private Region()
    {
    }

    public Region(Guid countryId, string name)
    {
        Id = Guid.CreateVersion7();
        CountryId = Guard.NotEmpty(countryId, nameof(countryId));
        Name = Guard.Required(name, nameof(name), NameMaxLength);
    }

    public Guid Id { get; private set; }

    public Guid CountryId { get; private set; }

    public Country? Country { get; private set; }

    public string Name { get; private set; } = string.Empty;
}
