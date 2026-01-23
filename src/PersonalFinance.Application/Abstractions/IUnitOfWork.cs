
namespace PersonalFinance.Application.Abstractions;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct);
}
