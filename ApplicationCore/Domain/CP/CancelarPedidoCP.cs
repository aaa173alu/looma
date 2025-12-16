using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using System;
using System.Linq;

namespace ApplicationCore.Domain.CP;

/// <summary>
/// CP: Cancelar pedido y restaurar stock
/// Custom Transaction
/// </summary>
public class CancelarPedidoCP
{
    private readonly IPedidoRepository _pedidoRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public CancelarPedidoCP(
        IPedidoRepository pedidoRepo,
        IProductoRepository productoRepo,
        IUnitOfWork uow)
    {
        _pedidoRepo = pedidoRepo;
        _productoRepo = productoRepo;
        _uow = uow;
    }

    public void Ejecutar(long pedidoId)
    {
        _uow.BeginTransaction();

        try
        {
            // 1. Obtener pedido
            Pedido? pedido = _pedidoRepo.GetById(pedidoId);
            if (pedido == null)
                throw new Exception($"Pedido con ID {pedidoId} no encontrado");

            // 2. Verificar que el pedido se puede cancelar (solo realizado o comprado)
            if (pedido.Estado != EstadoPedido.realizado && pedido.Estado != EstadoPedido.comprado)
                throw new Exception($"El pedido en estado '{pedido.Estado}' no se puede cancelar");

            // 3. Restaurar stock de cada producto
            foreach (ItemPedido item in pedido.Items)
            {
                Producto? producto = _productoRepo.GetById(item.ProductoId);
                if (producto != null)
                {
                    producto.Stock += item.Cantidad;
                    _productoRepo.Modify(producto);
                }
            }

            // 4. Cambiar estado del pedido a rechazado
            pedido.Estado = EstadoPedido.rechazado;
            _pedidoRepo.Modify(pedido);

            // 5. Guardar todo
            _uow.SaveChanges();
            _uow.Commit();
        }
        catch (Exception)
        {
            _uow.Rollback();
            throw;
        }
    }
}
