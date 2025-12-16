using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface ITarjetaRepository : IRepository<Tarjeta, long>
{
    IEnumerable<Tarjeta> GetByUsuario(long usuarioId);
    Tarjeta? GetPredeterminada(long usuarioId);
    void LimpiarPredeterminada(long usuarioId);
}
