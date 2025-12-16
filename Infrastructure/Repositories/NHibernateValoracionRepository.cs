using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateValoracionRepository : IValoracionRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateValoracionRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Valoracion New(Valoracion entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Valoracion entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Valoracion e = _uow.Session.Get<Valoracion>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Valoracion? GetById(long id) => _uow.Session.Get<Valoracion>(id);

    public IEnumerable<Valoracion> GetAll() => _uow.Session.Query<Valoracion>().ToList();

    public IEnumerable<Valoracion> ReadFilter(long? usuarioId = null, long? productoId = null, int? valorMin = null, int? valorMax = null)
    {
        string hql = "from Valoracion v where 1=1";
        if (usuarioId.HasValue) hql += " and v.UsuarioId = :usuarioId";
        if (productoId.HasValue) hql += " and v.ProductoId = :productoId";
        if (valorMin.HasValue) hql += " and v.Valor >= :valorMin";
        if (valorMax.HasValue) hql += " and v.Valor <= :valorMax";
        global::NHibernate.IQuery q = _uow.Session.CreateQuery(hql);
        if (usuarioId.HasValue) q.SetParameter("usuarioId", usuarioId.Value);
        if (productoId.HasValue) q.SetParameter("productoId", productoId.Value);
        if (valorMin.HasValue) q.SetParameter("valorMin", valorMin.Value);
        if (valorMax.HasValue) q.SetParameter("valorMax", valorMax.Value);
        return q.List<Valoracion>();
    }
}
