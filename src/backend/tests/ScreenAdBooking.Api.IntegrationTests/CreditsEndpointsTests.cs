namespace ScreenAdBooking.Api.IntegrationTests;

public sealed class CreditsEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    private const string DemoPassword = "TestDemo123!";

    [Fact]
    public async Task Lists_the_credit_packs()
    {
        var client = await LoggedInClientAsync("lina.chevalier@example.com");

        var packs = await client.GetFromJsonAsync<List<CreditPackResponse>>("/api/credits/packs");

        Assert.Equal(["starter", "standard", "large"], packs!.Select(p => p.Code));
    }

    [Fact]
    public async Task Demo_advertiser_starts_with_its_seeded_top_up()
    {
        var client = await LoggedInClientAsync("paul.mercier@example.com");

        var wallet = await client.GetFromJsonAsync<WalletResponse>("/api/credits");

        Assert.Equal((2000, 0), (wallet!.Available, wallet.Held));
        Assert.Equal("Standard pack top-up", Assert.Single(wallet.Movements).Label);
    }

    [Fact]
    public async Task New_advertiser_tops_up_and_sees_the_movement()
    {
        var client = await RegisteredClientAsync("fresh.juice@example.com");
        Assert.Empty((await client.GetFromJsonAsync<WalletResponse>("/api/credits"))!.Movements);

        var response = await client.PostAsJsonAsync("/api/credits/top-up", new TopUpRequest("large"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var wallet = await response.Content.ReadFromJsonAsync<WalletResponse>();
        Assert.Equal(5000, wallet!.Available);
        var movement = Assert.Single(wallet.Movements);
        Assert.Equal(("TopUp", 5000), (movement.Type, movement.Credits));
        Assert.Equal(wallet, await client.GetFromJsonAsync<WalletResponse>("/api/credits"), new WalletComparer());
    }

    [Fact]
    public async Task Top_up_with_an_unknown_pack_is_a_validation_problem()
    {
        var client = await LoggedInClientAsync("lina.chevalier@example.com");

        var response = await client.PostAsJsonAsync("/api/credits/top-up", new TopUpRequest("huge"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["Choose a credit pack."], problem!.Errors["packCode"]);
    }

    [Fact]
    public async Task Agency_admin_has_no_wallet()
    {
        var client = await LoggedInClientAsync("nora.vidal@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/credits")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/credits/top-up", new TopUpRequest("starter"))).StatusCode);
    }

    [Fact]
    public async Task Unknown_or_anonymous_users_are_refused()
    {
        var ghost = new User("ghost@example.com", "Ghost", "hash", UserRole.Advertiser, Guid.NewGuid(), DateTime.UtcNow);
        var unknown = ClientWithToken(factory.Services.GetRequiredService<IAccessTokenIssuer>().Issue(ghost).Value);
        var withoutSubject = ClientWithToken(TokenWithoutSubject());

        foreach (var client in new[] { unknown, withoutSubject, factory.CreateClient() })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/credits")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/credits/top-up", new TopUpRequest("starter"))).StatusCode);
        }
    }

    private HttpClient ClientWithToken(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<HttpClient> RegisteredClientAsync(string email) => await AuthorizeAsync(
        await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("Fresh Juice Co", "Test User", email, DemoPassword, DemoPassword)));

    private async Task<HttpClient> LoggedInClientAsync(string email) => await AuthorizeAsync(
        await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, DemoPassword)));

    private async Task<HttpClient> AuthorizeAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return ClientWithToken(body!.AccessToken);
    }

    private string TokenWithoutSubject()
    {
        var settings = factory.Services.GetRequiredService<JwtSettings>();

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = new Dictionary<string, object> { [JwtClaims.Email] = "ghost@example.com" },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(settings.SigningKey), SecurityAlgorithms.HmacSha256),
        });
    }

    private sealed class WalletComparer : IEqualityComparer<WalletResponse?>
    {
        public bool Equals(WalletResponse? x, WalletResponse? y) =>
            x!.Available == y!.Available && x.Held == y.Held && x.Movements.SequenceEqual(y.Movements);

        public int GetHashCode(WalletResponse? obj) => 0;
    }
}
