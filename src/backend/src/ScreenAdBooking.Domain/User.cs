namespace ScreenAdBooking.Domain;

public sealed class User
{
    public const int EmailMaxLength = 254;
    public const int DisplayNameMaxLength = 100;
    public const int PasswordHashMaxLength = 500;

    private User()
    {
    }

    public User(string email, string displayName, string passwordHash, UserRole role, Guid? companyId, DateTime createdAtUtc)
    {
        Id = Guid.CreateVersion7();
        Email = Guard.Email(email, nameof(email), EmailMaxLength);
        DisplayName = Guard.Required(displayName, nameof(displayName), DisplayNameMaxLength);
        PasswordHash = Guard.Required(passwordHash, nameof(passwordHash), PasswordHashMaxLength);
        Role = Guard.Defined(role, nameof(role));
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));

        var hasCompany = companyId is { } id && id != Guid.Empty;
        if (Role == UserRole.Advertiser && !hasCompany)
        {
            throw new ArgumentException("An advertiser belongs to a company.", nameof(companyId));
        }

        if (Role == UserRole.AgencyAdmin && companyId is not null)
        {
            throw new ArgumentException("Agency admins do not belong to a company.", nameof(companyId));
        }

        CompanyId = companyId;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    /// <summary>Set for advertisers only.</summary>
    public Guid? CompanyId { get; private set; }

    public Company? Company { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static bool IsValidEmail([NotNullWhen(true)] string? email) => Guard.IsEmail(email, EmailMaxLength);

    public void ChangePassword(string passwordHash) =>
        PasswordHash = Guard.Required(passwordHash, nameof(passwordHash), PasswordHashMaxLength);
}
