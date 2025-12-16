using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernatePedidoRepository : IPedidoRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernatePedidoRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Pedido New(Pedido entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Destroy(long id)
    {
        Pedido e = _uow.Session.Get<Pedido>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public IEnumerable<Pedido> GetAll() => _uow.Session.Query<Pedido>().ToList();

    public Pedido? GetById(long id) => _uow.Session.Get<Pedido>(id);

    public IEnumerable<Pedido> GetByUsuario(long usuarioId) => _uow.Session.Query<Pedido>().Where(p => p.UsuarioId == usuarioId).ToList();

    public void Modify(Pedido entity)
    {
        var merged = _uow.Session.Merge(entity);
        _uow.Session.Update(merged);
    }

    public IEnumerable<Pedido> ReadFilter(
        long? usuarioId = null,
        ApplicationCore.Domain.Enums.EstadoPedido? estado = null,
        System.DateTime? fechaDesde = null,
        System.DateTime? fechaHasta = null,
        decimal? totalMin = null,
        decimal? totalMax = null)
    {
        string hql = "from Pedido p where 1=1";

        if (usuarioId.HasValue) hql += " and p.UsuarioId = :usuarioId";
        if (estado.HasValue) hql += " and p.Estado = :estado";
        if (fechaDesde.HasValue) hql += " and p.Fecha >= :fechaDesde";
        if (fechaHasta.HasValue) hql += " and p.Fecha <= :fechaHasta";
        if (totalMin.HasValue) hql += " and p.Total >= :totalMin";
        if (totalMax.HasValue) hql += " and p.Total <= :totalMax";

        hql += " order by p.Fecha desc";

        IQuery q = _uow.Session.CreateQuery(hql);
        if (usuarioId.HasValue) q.SetParameter("usuarioId", usuarioId.Value);
        if (estado.HasValue) q.SetParameter("estado", estado.Value);
        if (fechaDesde.HasValue) q.SetParameter("fechaDesde", fechaDesde.Value);
        if (fechaHasta.HasValue) q.SetParameter("fechaHasta", fechaHasta.Value);
        if (totalMin.HasValue) q.SetParameter("totalMin", totalMin.Value);
        if (totalMax.HasValue) q.SetParameter("totalMax", totalMax.Value);

        return q.List<Pedido>();
    }
}
