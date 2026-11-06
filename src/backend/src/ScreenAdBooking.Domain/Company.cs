namespace ScreenAdBooking.Domain;

/// <summary>An advertiser company.</summary>
public sealed class Company
{
    public const int NameMaxLength = 150;
    public const int SectorMaxLength = 100;

    private Company()
    {
    }

    public Company(string name, string? sector, DateTime createdAtUtc)
    {
        Id = Guid.CreateVersion7();
        Name = Guard.Required(name, nameof(name), NameMaxLength);
        Sector = Guard.Optional(sector, nameof(sector), SectorMaxLength);
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Sector { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
