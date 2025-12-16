using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System.Collections.Generic;

namespace ApplicationCore.Domain.CP;

public class FinalizarCompraCP
{
    private readonly PedidoCEN _pedidoCEN;
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public FinalizarCompraCP(PedidoCEN pedidoCEN, IProductoRepository productoRepo, IUnitOfWork uow)
    {
        _pedidoCEN = pedidoCEN;
        _productoRepo = productoRepo;
        _uow = uow;
    }

    public Pedido Execute(long usuarioId, string direccion, List<ItemPedido> items)
    {
        _uow.BeginTransaction();
        try
        {
            // Validate stock
            foreach (ItemPedido it in items)
            {
                Producto? prod = _productoRepo.GetById(it.ProductoId);
                if (prod == null) throw new System.Exception($"Producto {it.ProductoId} no existe");
                if (prod.Stock < it.Cantidad) throw new System.Exception($"Stock insuficiente para {prod.Nombre}");
                prod.Stock -= it.Cantidad;
                _productoRepo.Modify(prod);
            }

            Pedido pedido = _pedidoCEN.CrearPedido(usuarioId, direccion, items);
            _uow.SaveChanges();
            _uow.Commit();
            return pedido;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
