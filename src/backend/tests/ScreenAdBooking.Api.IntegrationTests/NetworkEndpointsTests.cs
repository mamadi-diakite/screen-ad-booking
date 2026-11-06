using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ScreenAdBooking.Application.Contracts.Auth;
using ScreenAdBooking.Application.Contracts.Network;

namespace ScreenAdBooking.Api.IntegrationTests;

public sealed class NetworkEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    [Fact]
    public async Task Advertiser_gets_the_locations_for_the_filters()
    {
        var client = await LoggedInClientAsync("paul.mercier@example.com");

        var locations = await client.GetFromJsonAsync<List<NetworkCountry>>("/api/network/locations");

        Assert.NotNull(locations);
        Assert.Equal(["BE", "FR"], locations.Select(c => c.Code));
        Assert.All(locations, c => Assert.NotEmpty(c.Regions));
    }

    [Fact]
    public async Task Advertiser_searches_venues_with_filters_and_paging()
    {
        var client = await LoggedInClientAsync("lina.chevalier@example.com");

        var all = await client.GetFromJsonAsync<PagedResponse<VenueSummary>>("/api/venues?pageSize=5&page=2");
        var filtered = await client.GetFromJsonAsync<PagedResponse<VenueSummary>>(
            "/api/venues?country=fr&type=Station&footfall=High&locationType=TransportHub");

        Assert.Equal((2, 5, 26), (all!.Page, all.PageSize, all.TotalCount));
        Assert.Equal(5, all.Items.Count);
        Assert.Equal(["Hilltop Station Hall", "North Station Concourse"], filtered!.Items.Select(v => v.Name));
    }

    [Fact]
    public async Task Agency_admin_filters_by_city()
    {
        var client = await LoggedInClientAsync("nora.vidal@example.com");
        var locations = await client.GetFromJsonAsync<List<NetworkCountry>>("/api/network/locations");
        var liege = locations!.Single(c => c.Code == "BE").Regions.SelectMany(r => r.Cities).Single(c => c.Name == "Liège");

        var venues = await client.GetFromJsonAsync<PagedResponse<VenueSummary>>($"/api/venues?cityId={liege.Id}");

        Assert.Equal(2, venues!.TotalCount);
        Assert.All(venues.Items, v => Assert.Equal("Wallonia", v.RegionName));
    }

    [Fact]
    public async Task Unknown_filter_values_return_a_validation_problem()
    {
        var client = await LoggedInClientAsync("paul.mercier@example.com");

        var response = await client.GetAsync("/api/venues?country=France&type=Museum");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["country", "type"], problem!.Errors.Keys.Order());
    }

    [Theory]
    [InlineData("/api/network/locations")]
    [InlineData("/api/venues")]
    public async Task Network_needs_a_token(string path)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync(path)).StatusCode);
    }

    private async Task<HttpClient> LoggedInClientAsync(string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "TestDemo123!"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);

        return client;
    }
}
