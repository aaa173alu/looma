using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface ICarritoRepository : IRepository<Carrito, long>
{
    IEnumerable<Carrito> ReadFilter(long? usuarioId = null, bool? conItems = null);
}
