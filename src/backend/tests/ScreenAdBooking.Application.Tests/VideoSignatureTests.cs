namespace ScreenAdBooking.Application.Tests;

public sealed class VideoSignatureTests
{
    [Fact]
    public void Detects_mp4_and_webm_headers()
    {
        Assert.Equal(VideoFormat.Mp4, VideoSignature.Detect(VideoBytes.Mp4()));
        Assert.Equal(VideoFormat.Mp4, VideoSignature.Detect(VideoBytes.Mp4(length: 12)));
        Assert.Equal(VideoFormat.WebM, VideoSignature.Detect(VideoBytes.WebM()));
        Assert.Equal(VideoFormat.WebM, VideoSignature.Detect(VideoBytes.WebM(length: 4096)));
    }

    [Fact]
    public void Rejects_other_content()
    {
        Assert.Null(VideoSignature.Detect([]));
        Assert.Null(VideoSignature.Detect(VideoBytes.Mp4()[..11]));
        Assert.Null(VideoSignature.Detect(VideoBytes.QuickTime()));
        Assert.Null(VideoSignature.Detect(VideoBytes.Matroska()));
        Assert.Null(VideoSignature.Detect("GIF89a, not a video at all"u8));
        Assert.Null(VideoSignature.Detect([0x1A, 0x45, 0xDF, 0xA3]));
    }

    [Fact]
    public void Looks_for_the_webm_doc_type_in_the_header_only()
    {
        var late = new byte[VideoSignature.HeaderLength + 8];
        new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }.CopyTo(late, 0);
        "webm"u8.CopyTo(late.AsSpan(VideoSignature.HeaderLength));

        Assert.Null(VideoSignature.Detect(late));
    }
}
