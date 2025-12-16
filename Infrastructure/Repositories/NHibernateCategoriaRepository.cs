using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateCategoriaRepository : ICategoriaRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateCategoriaRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Categoria New(Categoria entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Categoria entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Categoria e = _uow.Session.Get<Categoria>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Categoria? GetById(long id) => _uow.Session.Get<Categoria>(id);

    public IEnumerable<Categoria> GetAll() => _uow.Session.Query<Categoria>().ToList();

    public IEnumerable<Categoria> ReadFilter(string nombre = null)
    {
        string hql = "from Categoria c where 1=1";
        if (!string.IsNullOrEmpty(nombre)) hql += " and c.Nombre like :nombre";
        global::NHibernate.IQuery q = _uow.Session.CreateQuery(hql);
        if (!string.IsNullOrEmpty(nombre)) q.SetParameter("nombre", "%" + nombre + "%");
        return q.List<Categoria>();
    }
}
