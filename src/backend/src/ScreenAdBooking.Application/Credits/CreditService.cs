namespace ScreenAdBooking.Application.Credits;

public enum WalletStatus
{
    Ok,
    UserNotFound,
    NotAdvertiser,
    Invalid,
}

/// <param name="Errors">Messages by field name in camelCase when <paramref name="Status"/> is Invalid.</param>
public sealed record WalletResult(WalletStatus Status, WalletResponse? Wallet, IReadOnlyDictionary<string, string[]> Errors)
{
    public static WalletResult Of(WalletStatus status) => new(status, null, new Dictionary<string, string[]>());
}

public sealed class CreditService(IUserRepository users, ICreditRepository credits, TimeProvider timeProvider)
{
    public static IReadOnlyList<CreditPackResponse> Packs { get; } =
        [.. CreditPacks.All.Select(pack => new CreditPackResponse(pack.Code, pack.Name, pack.Credits))];

    public async Task<WalletResult> GetWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (status, companyId) = await FindCompanyAsync(userId, cancellationToken);

        return status == WalletStatus.Ok
            ? new WalletResult(status, await WalletAsync(companyId, cancellationToken), new Dictionary<string, string[]>())
            : WalletResult.Of(status);
    }

    /// <summary>Adds the credits of a pack for free. No payment is involved.</summary>
    public async Task<WalletResult> TopUpAsync(Guid userId, TopUpRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (status, companyId) = await FindCompanyAsync(userId, cancellationToken);
        if (status != WalletStatus.Ok)
        {
            return WalletResult.Of(status);
        }

        if (CreditPacks.Find(request.PackCode) is not { } pack)
        {
            return new WalletResult(WalletStatus.Invalid, null, new Dictionary<string, string[]>
            {
                ["packCode"] = ["Choose a credit pack."],
            });
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        credits.Add(new CreditMovement(companyId, CreditMovementType.TopUp, pack.Credits, $"{pack.Name} pack top-up", now));
        await credits.SaveChangesAsync(cancellationToken);

        return new WalletResult(WalletStatus.Ok, await WalletAsync(companyId, cancellationToken), new Dictionary<string, string[]>());
    }

    private async Task<(WalletStatus Status, Guid CompanyId)> FindCompanyAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);

        return user switch
        {
            null => (WalletStatus.UserNotFound, Guid.Empty),
            { CompanyId: { } companyId } => (WalletStatus.Ok, companyId),
            _ => (WalletStatus.NotAdvertiser, Guid.Empty),
        };
    }

    private async Task<WalletResponse> WalletAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var movements = await credits.ListAsync(companyId, cancellationToken);
        var balance = CreditBalance.From(movements);

        return new WalletResponse(
            balance.Available,
            balance.Held,
            [.. movements.Select(m => new CreditMovementResponse(m.Id, m.Type.ToString(), m.Credits, m.Label, m.CreatedAtUtc))]);
    }
}
