namespace ScreenAdBooking.Domain;

public sealed class Country
{
    public const int NameMaxLength = 100;
    public const int CodeLength = 2;

    private Country()
    {
    }

    public Country(string code, string name)
    {
        var normalizedCode = Guard.Required(code, nameof(code), CodeLength).ToUpperInvariant();
        if (normalizedCode.Length != CodeLength || !normalizedCode.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("Country code must be two letters (ISO 3166-1).", nameof(code));
        }

        Id = Guid.CreateVersion7();
        Code = normalizedCode;
        Name = Guard.Required(name, nameof(name), NameMaxLength);
    }

    public Guid Id { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 code, e.g. FR.</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
}
