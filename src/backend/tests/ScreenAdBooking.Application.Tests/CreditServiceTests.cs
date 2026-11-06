namespace ScreenAdBooking.Application.Tests;

public sealed class CreditServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserRepository users = new();
    private readonly InMemoryCreditRepository credits = new();
    private readonly User advertiser;
    private readonly User admin;

    public CreditServiceTests()
    {
        var company = new Company("Fresh Juice Co", null, Now.UtcDateTime);
        advertiser = new User("ada@example.com", "Ada", "hash", UserRole.Advertiser, company.Id, Now.UtcDateTime);
        admin = new User("admin@example.com", "Admin", "hash", UserRole.AgencyAdmin, null, Now.UtcDateTime);
        users.Add(advertiser);
        users.Add(admin);
    }

    [Fact]
    public void Lists_the_packs()
    {
        Assert.Equal(
            [new("starter", "Starter", 500), new("standard", "Standard", 2000), new("large", "Large", 5000)],
            CreditService.Packs);
    }

    [Fact]
    public async Task New_wallet_is_empty()
    {
        var result = await Service().GetWalletAsync(advertiser.Id);

        Assert.Equal(WalletStatus.Ok, result.Status);
        Assert.Equal((0, 0), (result.Wallet!.Available, result.Wallet.Held));
        Assert.Empty(result.Wallet.Movements);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Top_up_adds_the_pack_credits_and_shows_newest_movement_first()
    {
        await Service(Now).TopUpAsync(advertiser.Id, new TopUpRequest("starter"));
        var result = await Service(Now.AddMinutes(1)).TopUpAsync(advertiser.Id, new TopUpRequest(" Large "));

        Assert.Equal(WalletStatus.Ok, result.Status);
        Assert.Equal(5500, result.Wallet!.Available);
        Assert.Equal(0, result.Wallet.Held);
        Assert.Equal(["Large pack top-up", "Starter pack top-up"], result.Wallet.Movements.Select(m => m.Label));
        var latest = result.Wallet.Movements[0];
        Assert.Equal(("TopUp", 5000, Now.AddMinutes(1).UtcDateTime), (latest.Type, latest.Credits, latest.CreatedAtUtc));
        Assert.Equal(2, credits.SaveCount);
        Assert.Equal(5500, (await Service().GetWalletAsync(advertiser.Id)).Wallet!.Available);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("huge")]
    public async Task Top_up_needs_a_known_pack(string? packCode)
    {
        var result = await Service().TopUpAsync(advertiser.Id, new TopUpRequest(packCode));

        Assert.Equal(WalletStatus.Invalid, result.Status);
        Assert.Null(result.Wallet);
        Assert.Equal(["Choose a credit pack."], result.Errors["packCode"]);
        Assert.Empty(credits.Movements);
    }

    [Fact]
    public async Task Agency_admins_have_no_wallet()
    {
        Assert.Equal(WalletStatus.NotAdvertiser, (await Service().GetWalletAsync(admin.Id)).Status);
        Assert.Equal(WalletStatus.NotAdvertiser, (await Service().TopUpAsync(admin.Id, new TopUpRequest("starter"))).Status);
        Assert.Empty(credits.Movements);
    }

    [Fact]
    public async Task Unknown_user_is_reported()
    {
        var result = await Service().GetWalletAsync(Guid.NewGuid());

        Assert.Equal(WalletStatus.UserNotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal(WalletStatus.UserNotFound, (await Service().TopUpAsync(Guid.NewGuid(), new TopUpRequest("starter"))).Status);
    }

    [Fact]
    public async Task Top_up_rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => Service().TopUpAsync(advertiser.Id, null!));
    }

    private CreditService Service(DateTimeOffset? now = null) => new(users, credits, new FixedTimeProvider(now ?? Now));
}
