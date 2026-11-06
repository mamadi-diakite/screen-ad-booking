namespace ScreenAdBooking.Application.Credits;

public interface ICreditRepository
{
    /// <returns>Every movement of the company, newest first.</returns>
    Task<IReadOnlyList<CreditMovement>> ListAsync(Guid companyId, CancellationToken cancellationToken);

    void Add(CreditMovement movement);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
