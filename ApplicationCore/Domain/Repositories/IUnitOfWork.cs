namespace ApplicationCore.Domain.Repositories;

public interface IUnitOfWork
{
    void BeginTransaction();
    void Commit();
    void Rollback();
    void SaveChanges();
}
