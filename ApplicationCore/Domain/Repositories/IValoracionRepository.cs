using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IValoracionRepository : IRepository<Valoracion, long>
{
    IEnumerable<Valoracion> ReadFilter(long? usuarioId = null, long? productoId = null, int? valorMin = null, int? valorMax = null);
}
