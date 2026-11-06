namespace ScreenAdBooking.Infrastructure.Persistence;

public sealed class VideoRepository(ScreenAdBookingDbContext db) : IVideoRepository
{
    public async Task<IReadOnlyList<Video>> ListAsync(Guid companyId, CancellationToken cancellationToken) =>
        await db.Videos
            .AsNoTracking()
            .Where(v => v.CompanyId == companyId)
            .OrderByDescending(v => v.UploadedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Video?> FindAsync(Guid companyId, Guid videoId, CancellationToken cancellationToken) =>
        db.Videos.SingleOrDefaultAsync(v => v.Id == videoId && v.CompanyId == companyId, cancellationToken);

    public void Add(Video video) => db.Videos.Add(video);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
