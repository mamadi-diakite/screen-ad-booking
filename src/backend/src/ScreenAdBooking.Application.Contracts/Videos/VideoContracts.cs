namespace ScreenAdBooking.Application.Contracts.Videos;

/// <param name="Format">Mp4 or WebM.</param>
/// <param name="Status">PendingReview, Approved or Rejected.</param>
/// <param name="RejectionReason">Why the agency rejected the video; null otherwise.</param>
public sealed record VideoResponse(
    Guid Id,
    string Name,
    string Format,
    long SizeBytes,
    string Status,
    string? RejectionReason,
    DateTime UploadedAtUtc);
