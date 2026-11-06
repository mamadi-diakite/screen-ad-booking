using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Application.Credits;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Persistence;

public sealed class CreditRepository(ScreenAdBookingDbContext db) : ICreditRepository
{
    public async Task<IReadOnlyList<CreditMovement>> ListAsync(Guid companyId, CancellationToken cancellationToken) =>
        await db.CreditMovements
            .AsNoTracking()
            .Where(m => m.CompanyId == companyId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(CreditMovement movement) => db.CreditMovements.Add(movement);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
