using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class InMemoryProductoRepository : IProductoRepository
{
    private readonly ConcurrentDictionary<long, Producto> _store = new();
    private long _seq = 1;

    public Producto New(Producto entity)
    {
        long id = System.Threading.Interlocked.Increment(ref _seq);
        entity.Id = id;
        _store[id] = entity;
        return entity;
    }

    public void Destroy(long id) => _store.TryRemove(id, out _);

    public IEnumerable<Producto> GetAll() => _store.Values.ToList();

    public Producto? GetById(long id) => _store.TryGetValue(id, out Producto? p) ? p : null;

    public IEnumerable<Producto> GetDestacados() => _store.Values.Where(p => p.Destacado).ToList();

    public IEnumerable<Producto> ReadFilter(
        decimal? precioMin = null,
        decimal? precioMax = null,
        int? stockMin = null,
        bool? destacado = null,
        string nombre = null,
        string color = null)
    {
        IEnumerable<Producto> q = _store.Values;
        if (precioMin.HasValue) q = q.Where(p => p.Precio >= precioMin.Value);
        if (precioMax.HasValue) q = q.Where(p => p.Precio <= precioMax.Value);
        if (stockMin.HasValue) q = q.Where(p => p.Stock >= stockMin.Value);
        if (destacado.HasValue) q = q.Where(p => p.Destacado == destacado.Value);
        if (!string.IsNullOrEmpty(nombre)) q = q.Where(p => p.Nombre != null && p.Nombre.Contains(nombre));
        if (!string.IsNullOrEmpty(color)) q = q.Where(p => p.Color != null && p.Color.ToLower().Contains(color.ToLower()));
        return q.ToList();
    }

    public void Modify(Producto entity)
    {
        if (_store.ContainsKey(entity.Id)) _store[entity.Id] = entity;
    }
}
