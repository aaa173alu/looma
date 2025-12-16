using ApplicationCore.Domain.Repositories;

namespace Infrastructure.UnitOfWork;

public class InMemoryUnitOfWork : IUnitOfWork
{
    // In-memory implementation: no real transaction support; methods are no-ops.
    public void BeginTransaction() { }
    public void Commit() { }
    public void Rollback() { }
    public void SaveChanges() { }
}
