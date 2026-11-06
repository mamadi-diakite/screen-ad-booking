namespace ScreenAdBooking.Domain;

public enum CreditMovementType
{
    TopUp,
    Hold,
    Charge,
    Release,
}

/// <summary>One change to a company's fictional credits. Credits are whole numbers.</summary>
public sealed class CreditMovement
{
    public const int LabelMaxLength = 200;

    private CreditMovement()
    {
    }

    public CreditMovement(Guid companyId, CreditMovementType type, int credits, string label, DateTime createdAtUtc)
    {
        Id = Guid.CreateVersion7();
        CompanyId = Guard.NotEmpty(companyId, nameof(companyId));
        Type = Guard.Defined(type, nameof(type));
        Credits = Guard.Positive(credits, nameof(credits));
        Label = Guard.Required(label, nameof(label), LabelMaxLength);
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public CreditMovementType Type { get; private set; }

    /// <summary>Always positive; <see cref="Type"/> tells which way the credits move.</summary>
    public int Credits { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }
}

/// <param name="Available">Credits that can be spent on new bookings.</param>
/// <param name="Held">Credits set aside for booked slots that have not been played yet.</param>
public readonly record struct CreditBalance(int Available, int Held)
{
    public static CreditBalance From(IEnumerable<CreditMovement> movements)
    {
        var balance = new CreditBalance(0, 0);
        foreach (var movement in movements)
        {
            balance = balance.Apply(movement);
        }

        return balance;
    }

    private CreditBalance Apply(CreditMovement movement) => movement.Type switch
    {
        CreditMovementType.TopUp => this with { Available = Available + movement.Credits },
        CreditMovementType.Hold => new(Available - movement.Credits, Held + movement.Credits),
        CreditMovementType.Charge => this with { Held = Held - movement.Credits },
        _ => new(Available + movement.Credits, Held - movement.Credits),
    };
}

public sealed record CreditPack(string Code, string Name, int Credits);

/// <summary>Packs an advertiser picks from to top up for free. No payment is involved.</summary>
public static class CreditPacks
{
    public static IReadOnlyList<CreditPack> All { get; } =
    [
        new("starter", "Starter", 500),
        new("standard", "Standard", 2000),
        new("large", "Large", 5000),
    ];

    public static CreditPack? Find(string? code) =>
        All.FirstOrDefault(pack => string.Equals(pack.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase));
}
