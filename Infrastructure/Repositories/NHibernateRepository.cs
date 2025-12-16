using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly NHibernateUnitOfWork _uow;

    public NHibernateRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public TEntity New(TEntity entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(TEntity entity)
    {
        _uow.Session.Merge(entity);
    }

    public void Destroy(TKey id)
    {
        TEntity entity = _uow.Session.Get<TEntity>(id);
        if (entity != null)
        {
            _uow.Session.Delete(entity);
        }
    }

    public TEntity? GetById(TKey id)
    {
        return _uow.Session.Get<TEntity>(id);
    }

    public IEnumerable<TEntity> GetAll()
    {
        return _uow.Session.Query<TEntity>().ToList();
    }
}
