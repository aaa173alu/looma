using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateFavoritosRepository : IFavoritosRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateFavoritosRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Favoritos New(Favoritos entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Favoritos entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Favoritos e = _uow.Session.Get<Favoritos>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Favoritos? GetById(long id) => _uow.Session.Get<Favoritos>(id);

    public IEnumerable<Favoritos> GetAll() => _uow.Session.Query<Favoritos>().ToList();

    public IEnumerable<Favoritos> ReadFilter(long? usuarioId = null, long? productoId = null)
    {
        string hql = "from Favoritos f where 1=1";
        if (usuarioId.HasValue) hql += " and f.UsuarioId = :usuarioId";
        if (productoId.HasValue) hql += " and f.ProductoId = :productoId";
        global::NHibernate.IQuery q = _uow.Session.CreateQuery(hql);
        if (usuarioId.HasValue) q.SetParameter("usuarioId", usuarioId.Value);
        if (productoId.HasValue) q.SetParameter("productoId", productoId.Value);
        return q.List<Favoritos>();
    }
}
