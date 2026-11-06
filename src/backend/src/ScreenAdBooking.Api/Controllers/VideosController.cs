namespace ScreenAdBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/videos")]
public sealed class VideosController(VideoService videoService) : ControllerBase
{
    /// <summary>Leaves room for the multipart envelope around a 10 MB video.</summary>
    private const long MaxRequestBytes = Video.MaxSizeBytes + (1024 * 1024);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VideoResponse>>> List(CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var result = await videoService.ListAsync(userId, cancellationToken);

        return result.Status == VideoStatus.Ok ? Ok(result.Value) : Refused(result.Status);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<VideoResponse>> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        await using var content = file?.OpenReadStream();
        var result = await videoService.UploadAsync(userId, file?.FileName, content, cancellationToken);

        return result.Status switch
        {
            VideoStatus.Ok => CreatedAtAction(nameof(Preview), new { id = result.Value!.Id }, result.Value),
            VideoStatus.Invalid => this.InvalidRequest(result.Errors),
            _ => Refused(result.Status),
        };
    }

    /// <summary>Streams the video for preview, with range requests for seeking.</summary>
    [HttpGet("{id:guid}/content")]
    public async Task<ActionResult> Preview(Guid id, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var result = await videoService.OpenAsync(userId, id, cancellationToken);

        return result.Status == VideoStatus.Ok
            ? File(result.Value!.Content, result.Value.ContentType, enableRangeProcessing: true)
            : Refused(result.Status);
    }

    private ActionResult Refused(VideoStatus status) => status switch
    {
        VideoStatus.UserNotFound => Unauthorized(),
        VideoStatus.NotAdvertiser => Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Only advertisers have a video library."),
        _ => NotFound(),
    };
}
