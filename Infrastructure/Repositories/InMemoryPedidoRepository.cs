using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class InMemoryPedidoRepository : IPedidoRepository
{
    private readonly ConcurrentDictionary<long, Pedido> _store = new();
    private long _seq = 1;

    public Pedido New(Pedido entity)
    {
        long id = System.Threading.Interlocked.Increment(ref _seq);
        entity.Id = id;
        _store[id] = entity;
        return entity;
    }

    public void Destroy(long id) => _store.TryRemove(id, out _);

    public IEnumerable<Pedido> GetAll() => _store.Values.ToList();

    public Pedido? GetById(long id) => _store.TryGetValue(id, out Pedido? p) ? p : null;

    public IEnumerable<Pedido> GetByUsuario(long usuarioId) => _store.Values.Where(x => x.UsuarioId == usuarioId).ToList();

    public void Modify(Pedido entity)
    {
        if (_store.ContainsKey(entity.Id)) _store[entity.Id] = entity;
    }

    public IEnumerable<Pedido> ReadFilter(
        long? usuarioId = null,
        ApplicationCore.Domain.Enums.EstadoPedido? estado = null,
        System.DateTime? fechaDesde = null,
        System.DateTime? fechaHasta = null,
        decimal? totalMin = null,
        decimal? totalMax = null)
    {
        IEnumerable<Pedido> q = _store.Values;
        if (usuarioId.HasValue) q = q.Where(p => p.UsuarioId == usuarioId.Value);
        if (estado.HasValue) q = q.Where(p => p.Estado == estado.Value);
        if (fechaDesde.HasValue) q = q.Where(p => p.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue) q = q.Where(p => p.Fecha <= fechaHasta.Value);
        if (totalMin.HasValue) q = q.Where(p => p.Total >= totalMin.Value);
        if (totalMax.HasValue) q = q.Where(p => p.Total <= totalMax.Value);
        return q.OrderByDescending(p => p.Fecha).ToList();
    }
}
