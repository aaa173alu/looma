using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CP;

/// <summary>
/// CP: Agregar producto al carrito con validación de stock
/// Custom Transaction
/// </summary>
public class AgregarProductoAlCarritoCP
{
    private readonly IRepository<Carrito, long> _carritoRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public AgregarProductoAlCarritoCP(
        IRepository<Carrito, long> carritoRepo,
        IProductoRepository productoRepo,
        IUnitOfWork uow)
    {
        _carritoRepo = carritoRepo;
        _productoRepo = productoRepo;
        _uow = uow;
    }

    public void Ejecutar(long carritoId, long productoId, int cantidad)
    {
        _uow.BeginTransaction();

        try
        {
            // 1. Obtener carrito
            Carrito? carrito = _carritoRepo.GetById(carritoId);
            if (carrito == null)
                throw new Exception($"Carrito con ID {carritoId} no encontrado");

            // 2. Verificar stock del producto
            Producto? producto = _productoRepo.GetById(productoId);
            if (producto == null)
                throw new Exception($"Producto con ID {productoId} no encontrado");

            if (producto.Stock < cantidad)
                throw new Exception($"Stock insuficiente. Stock disponible: {producto.Stock}");

            // 3. Agregar item al carrito o actualizar cantidad
            ItemPedido? itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (itemExistente != null)
            {
                // Verificar stock para cantidad acumulada
                if (producto.Stock < itemExistente.Cantidad + cantidad)
                    throw new Exception($"Stock insuficiente. Stock disponible: {producto.Stock}");

                itemExistente.Cantidad += cantidad;
            }
            else
            {
                carrito.Items.Add(new ItemPedido
                {
                    ProductoId = productoId,
                    Cantidad = cantidad
                });
            }

            // 4. Guardar cambios
            _carritoRepo.Modify(carrito);
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
