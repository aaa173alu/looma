using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IFavoritosRepository : IRepository<Favoritos, long>
{
    IEnumerable<Favoritos> ReadFilter(long? usuarioId = null, long? productoId = null);
}
