using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;

namespace Infrastructure.Repositories;

public class NHibernateProductoRepository : IProductoRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateProductoRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Producto New(Producto entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Destroy(long id)
    {
        Producto e = _uow.Session.Get<Producto>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public IEnumerable<Producto> GetAll() => _uow.Session.Query<Producto>().ToList();

    public Producto? GetById(long id) => _uow.Session.Get<Producto>(id);

    public IEnumerable<Producto> GetDestacados() => _uow.Session.Query<Producto>().Where(p => p.Destacado).ToList();

    public IEnumerable<Producto> ReadFilter(
        decimal? precioMin = null,
        decimal? precioMax = null,
        int? stockMin = null,
        bool? destacado = null,
        string nombre = null,
        string color = null)
    {
        string hql = "from Producto p where 1=1";
        IQuery query = _uow.Session.CreateQuery(hql);

        if (precioMin.HasValue)
        {
            hql += " and p.Precio >= :precioMin";
        }
        if (precioMax.HasValue)
        {
            hql += " and p.Precio <= :precioMax";
        }
        if (stockMin.HasValue)
        {
            hql += " and p.Stock >= :stockMin";
        }
        if (destacado.HasValue)
        {
            hql += " and p.Destacado = :destacado";
        }
        if (!string.IsNullOrEmpty(nombre))
        {
            hql += " and p.Nombre like :nombre";
        }
        if (!string.IsNullOrEmpty(color))
        {
            hql += " and lower(p.Color) like :color";
        }

        query = _uow.Session.CreateQuery(hql);

        if (precioMin.HasValue) query.SetParameter("precioMin", precioMin.Value);
        if (precioMax.HasValue) query.SetParameter("precioMax", precioMax.Value);
        if (stockMin.HasValue) query.SetParameter("stockMin", stockMin.Value);
        if (destacado.HasValue) query.SetParameter("destacado", destacado.Value);
        if (!string.IsNullOrEmpty(nombre)) query.SetParameter("nombre", "%" + nombre + "%");
        if (!string.IsNullOrEmpty(color)) query.SetParameter("color", "%" + color.ToLower() + "%");

        return query.List<Producto>();
    }

    public void Modify(Producto entity) => _uow.Session.Merge(entity);
}
