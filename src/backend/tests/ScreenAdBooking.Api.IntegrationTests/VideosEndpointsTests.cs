namespace ScreenAdBooking.Api.IntegrationTests;

public sealed class VideosEndpointsTests(DemoDataApiFactory factory) : IClassFixture<DemoDataApiFactory>
{
    private const string DemoPassword = "TestDemo123!";

    private static readonly byte[] Mp4 = Padded([0, 0, 0, 0x18, .. "ftypisom"u8], 4096);

    private static readonly byte[] WebM = Padded([0x1A, 0x45, 0xDF, 0xA3, 0x9F, 0x42, 0x82, 0x84, .. "webm"u8], 2048);

    [Fact]
    public async Task Advertiser_uploads_lists_and_previews_videos()
    {
        var client = await LoggedInClientAsync("paul.mercier@example.com");
        Assert.Empty((await client.GetFromJsonAsync<List<VideoResponse>>("/api/videos"))!);

        var created = await UploadAsync(client, Mp4, "spring-sale.mp4", "video/mp4");
        await UploadAsync(client, WebM, "summer.webm", "video/webm");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var video = (await created.Content.ReadFromJsonAsync<VideoResponse>())!;
        Assert.Equal(("spring-sale", "Mp4", 4096L, "PendingReview"), (video.Name, video.Format, video.SizeBytes, video.Status));
        Assert.Equal($"/api/videos/{video.Id}/content", created.Headers.Location?.AbsolutePath);
        Assert.True(File.Exists(Path.Combine(factory.VideosPath, $"{video.Id:N}.mp4")));

        var library = await client.GetFromJsonAsync<List<VideoResponse>>("/api/videos");
        Assert.Equal(["summer", "spring-sale"], library!.Select(v => v.Name));

        var preview = await client.GetAsync(created.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.Equal("video/mp4", preview.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Mp4, await preview.Content.ReadAsByteArrayAsync());

        using var range = new HttpRequestMessage(HttpMethod.Get, created.Headers.Location);
        range.Headers.Range = new RangeHeaderValue(0, 11);
        var partial = await client.SendAsync(range);
        Assert.Equal(HttpStatusCode.PartialContent, partial.StatusCode);
        Assert.Equal(Mp4[..12], await partial.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Other_advertisers_cannot_preview_a_video()
    {
        var owner = await LoggedInClientAsync("paul.mercier@example.com");
        var other = await LoggedInClientAsync("lina.chevalier@example.com");
        var created = await UploadAsync(owner, WebM, "ad.webm", "video/webm");

        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync(created.Headers.Location)).StatusCode);
        Assert.DoesNotContain(
            (await other.GetFromJsonAsync<List<VideoResponse>>("/api/videos"))!,
            v => created.Headers.Location!.AbsolutePath.Contains(v.Id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task Rejects_a_renamed_file_or_a_missing_file()
    {
        var client = await LoggedInClientAsync("lina.chevalier@example.com");

        var renamed = await UploadAsync(client, "MZ fake executable"u8.ToArray(), "trailer.mp4", "video/mp4");
        using var noFile = new MultipartFormDataContent { { new StringContent("hello"), "note" } };
        var missing = await client.PostAsync("/api/videos", noFile);

        await AssertFileErrorAsync(renamed, "The file content is not an MP4 or WebM video.");
        await AssertFileErrorAsync(missing, "Choose a video file.");
    }

    [Fact]
    public async Task Rejects_a_video_above_10_mb()
    {
        var client = await LoggedInClientAsync("lina.chevalier@example.com");
        var large = Padded(Mp4, (int)Video.MaxSizeBytes + 1);

        await AssertFileErrorAsync(await UploadAsync(client, large, "long.mp4", "video/mp4"), "The video is larger than 10 MB.");
    }

    [Fact]
    public async Task Agency_admin_has_no_video_library()
    {
        var client = await LoggedInClientAsync("nora.vidal@example.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/videos")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await UploadAsync(client, Mp4, "ad.mp4", "video/mp4")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/videos/{Guid.NewGuid()}/content")).StatusCode);
    }

    [Fact]
    public async Task Unknown_or_anonymous_users_are_refused()
    {
        var ghost = new User("ghost@example.com", "Ghost", "hash", UserRole.Advertiser, Guid.NewGuid(), DateTime.UtcNow);
        var unknown = ClientWithToken(factory.Services.GetRequiredService<IAccessTokenIssuer>().Issue(ghost).Value);
        var withoutSubject = ClientWithToken(TokenWithoutSubject());

        foreach (var client in new[] { unknown, withoutSubject, factory.CreateClient() })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/videos")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await UploadAsync(client, Mp4, "ad.mp4", "video/mp4")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/videos/{Guid.NewGuid()}/content")).StatusCode);
        }
    }

    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, byte[] bytes, string fileName, string contentType)
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);

        return await client.PostAsync("/api/videos", form);
    }

    private static async Task AssertFileErrorAsync(HttpResponseMessage response, string message)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal([message], problem!.Errors["file"]);
    }

    private static byte[] Padded(byte[] header, int length)
    {
        var bytes = new byte[length];
        header.CopyTo(bytes, 0);
        return bytes;
    }

    private HttpClient ClientWithToken(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<HttpClient> LoggedInClientAsync(string email)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, DemoPassword));
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
}
