using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateCarritoRepository : ICarritoRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateCarritoRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Carrito New(Carrito entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Carrito entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Carrito e = _uow.Session.Get<Carrito>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Carrito? GetById(long id) => _uow.Session.Get<Carrito>(id);

    public IEnumerable<Carrito> GetAll() => _uow.Session.Query<Carrito>().ToList();

    public IEnumerable<Carrito> ReadFilter(long? usuarioId = null, bool? conItems = null)
    {
        string hql = "from Carrito c where 1=1";
        if (usuarioId.HasValue) hql += " and c.UsuarioId = :usuarioId";
        if (conItems.HasValue && conItems.Value) hql += " and size(c.Items) > 0";
        if (conItems.HasValue && !conItems.Value) hql += " and size(c.Items) = 0";

        global::NHibernate.IQuery q = _uow.Session.CreateQuery(hql);
        if (usuarioId.HasValue) q.SetParameter("usuarioId", usuarioId.Value);
        return q.List<Carrito>();
    }
}
