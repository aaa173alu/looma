using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido, long>
{
    IEnumerable<Pedido> GetByUsuario(long usuarioId);

    IEnumerable<Pedido> ReadFilter(
        long? usuarioId = null,
        ApplicationCore.Domain.Enums.EstadoPedido? estado = null,
        System.DateTime? fechaDesde = null,
        System.DateTime? fechaHasta = null,
        decimal? totalMin = null,
        decimal? totalMax = null);
}
