namespace ScreenAdBooking.Application.Videos;

/// <summary>Tells MP4 and WebM files apart from anything else by their first bytes.</summary>
public static class VideoSignature
{
    /// <summary>Enough bytes to hold the MP4 brand and the WebM document type.</summary>
    public const int HeaderLength = 64;

    private static ReadOnlySpan<byte> FileTypeBox => "ftyp"u8;

    private static ReadOnlySpan<byte> QuickTimeBrand => "qt  "u8;

    private static ReadOnlySpan<byte> EbmlMagic => [0x1A, 0x45, 0xDF, 0xA3];

    private static ReadOnlySpan<byte> WebMDocType => "webm"u8;

    /// <returns>The format, or null for any other content, including QuickTime and Matroska files.</returns>
    public static VideoFormat? Detect(ReadOnlySpan<byte> content)
    {
        var header = content[..Math.Min(content.Length, HeaderLength)];

        if (header.Length >= 12 && header[4..8].SequenceEqual(FileTypeBox) && !header[8..12].SequenceEqual(QuickTimeBrand))
        {
            return VideoFormat.Mp4;
        }

        if (header.StartsWith(EbmlMagic) && header.IndexOf(WebMDocType) >= 0)
        {
            return VideoFormat.WebM;
        }

        return null;
    }
}
