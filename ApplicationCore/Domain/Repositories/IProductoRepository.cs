using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IProductoRepository : IRepository<Producto, long>
{
    IEnumerable<Producto> GetDestacados();

    IEnumerable<Producto> ReadFilter(
        decimal? precioMin = null,
        decimal? precioMax = null,
        int? stockMin = null,
        bool? destacado = null,
        string nombre = null,
        string color = null);
}
