using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using System;

namespace ApplicationCore.Domain.CP;

/// <summary>
/// CP: Procesar devolución de producto y restaurar stock
/// Custom Transaction
/// </summary>
public class ProcesarDevolucionCP
{
    private readonly IPedidoRepository _pedidoRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public ProcesarDevolucionCP(
        IPedidoRepository pedidoRepo,
        IProductoRepository productoRepo,
        IUnitOfWork uow)
    {
        _pedidoRepo = pedidoRepo;
        _productoRepo = productoRepo;
        _uow = uow;
    }

    public void Ejecutar(long pedidoId, long productoId, int cantidad, string motivo)
    {
        _uow.BeginTransaction();

        try
        {
            // 1. Obtener pedido
            Pedido? pedido = _pedidoRepo.GetById(pedidoId);
            if (pedido == null)
                throw new Exception($"Pedido con ID {pedidoId} no encontrado");

            // 2. Verificar que el pedido está recibido o validado
            if (pedido.Estado != EstadoPedido.recibido && pedido.Estado != EstadoPedido.validado)
                throw new Exception($"Solo se pueden procesar devoluciones de pedidos recibidos o validados");

            // 3. Buscar el item en el pedido
            ItemPedido? item = pedido.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (item == null)
                throw new Exception($"El producto {productoId} no está en el pedido");

            if (item.Cantidad < cantidad)
                throw new Exception($"La cantidad a devolver ({cantidad}) excede la cantidad del pedido ({item.Cantidad})");

            // 4. Restaurar stock del producto
            Producto? producto = _productoRepo.GetById(productoId);
            if (producto == null)
                throw new Exception($"Producto con ID {productoId} no encontrado");

            producto.Stock += cantidad;
            _productoRepo.Modify(producto);

            // 5. Actualizar el item del pedido
            item.Cantidad -= cantidad;
            if (item.Cantidad == 0)
            {
                pedido.Items.Remove(item);
            }

            // 6. Si no quedan items, cambiar estado
            if (pedido.Items.Count == 0)
            {
                pedido.Estado = EstadoPedido.rechazado;
            }

            _pedidoRepo.Modify(pedido);

            // 7. Guardar cambios
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
