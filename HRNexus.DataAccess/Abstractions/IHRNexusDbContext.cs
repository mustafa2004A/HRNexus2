namespace HRNexus.DataAccess.Abstractions;

public interface IHRNexusDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
