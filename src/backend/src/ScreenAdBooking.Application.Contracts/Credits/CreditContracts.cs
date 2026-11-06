namespace ScreenAdBooking.Application.Contracts.Credits;

public sealed record CreditPackResponse(string Code, string Name, int Credits);

public sealed record TopUpRequest(string? PackCode);

/// <param name="Type">TopUp, Hold, Charge or Release.</param>
public sealed record CreditMovementResponse(Guid Id, string Type, int Credits, string Label, DateTime CreatedAtUtc);

/// <param name="Movements">Newest first.</param>
public sealed record WalletResponse(int Available, int Held, IReadOnlyList<CreditMovementResponse> Movements);
