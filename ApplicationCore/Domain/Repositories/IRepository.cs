using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IRepository<T, TKey> where T : class
{
    T? GetById(TKey id);
    IEnumerable<T> GetAll();
    T New(T entity);
    void Modify(T entity);
    void Destroy(TKey id);
}
