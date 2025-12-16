using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CEN;

public class CarritoCEN
{
    private readonly ICarritoRepository _carritoRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public CarritoCEN(ICarritoRepository carritoRepo, IProductoRepository productoRepo, IUnitOfWork uow)
    {
        _carritoRepo = carritoRepo;
        _productoRepo = productoRepo;
        _uow = uow;
    }

    // CRUD Básico

    public Carrito Crear(long usuarioId)
    {
        Carrito carrito = new Carrito
        {
            UsuarioId = usuarioId,
            Items = new List<ItemPedido>()
        };

        Carrito created = _carritoRepo.New(carrito);
        _uow.SaveChanges();
        return created;
    }

    public void Destroy(long id)
    {
        _carritoRepo.Destroy(id);
        _uow.SaveChanges();
    }

    public Carrito ReadOID(long id)
    {
        return _carritoRepo.GetById(id);
    }

    public IList<Carrito> ReadAll()
    {
        return _carritoRepo.GetAll().ToList();
    }

    // Operaciones Custom

    public Carrito ObtenerPorUsuario(long usuarioId)
    {
        return _carritoRepo.GetAll().FirstOrDefault(c => c.UsuarioId == usuarioId);
    }

    public void AgregarItem(long carritoId, long productoId, int cantidad)
    {
        Carrito? carrito = _carritoRepo.GetById(carritoId);
        if (carrito == null)
            throw new Exception($"Carrito con ID {carritoId} no encontrado");

        Producto? producto = _productoRepo.GetById(productoId);
        if (producto == null)
            throw new Exception($"Producto con ID {productoId} no encontrado");

        if (producto.Stock < cantidad)
            throw new Exception("Stock insuficiente");

        // Buscar si ya existe el item
        ItemPedido? itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
        if (itemExistente != null)
        {
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

        _carritoRepo.Modify(carrito);
        _uow.SaveChanges();
    }

    public void EliminarItem(long carritoId, long productoId)
    {
        Carrito? carrito = _carritoRepo.GetById(carritoId);
        if (carrito == null)
            throw new Exception($"Carrito con ID {carritoId} no encontrado");

        ItemPedido? item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
        if (item != null)
        {
            carrito.Items.Remove(item);
            _carritoRepo.Modify(carrito);
            _uow.SaveChanges();
        }
    }

    public void ActualizarCantidad(long carritoId, long productoId, int nuevaCantidad)
    {
        Carrito? carrito = _carritoRepo.GetById(carritoId);
        if (carrito == null)
            throw new Exception($"Carrito con ID {carritoId} no encontrado");

        ItemPedido? item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
        if (item == null)
            throw new Exception("Producto no encontrado en el carrito");

        Producto? producto = _productoRepo.GetById(productoId);
        if (producto.Stock < nuevaCantidad)
            throw new Exception("Stock insuficiente");

        item.Cantidad = nuevaCantidad;
        _carritoRepo.Modify(carrito);
        _uow.SaveChanges();
    }

    public void Vaciar(long carritoId)
    {
        Carrito? carrito = _carritoRepo.GetById(carritoId);
        if (carrito == null)
            throw new Exception($"Carrito con ID {carritoId} no encontrado");

        carrito.Items.Clear();
        _carritoRepo.Modify(carrito);
        _uow.SaveChanges();
    }

    public decimal CalcularTotal(long carritoId)
    {
        Carrito? carrito = _carritoRepo.GetById(carritoId);
        if (carrito == null)
            throw new Exception($"Carrito con ID {carritoId} no encontrado");

        decimal total = 0;
        foreach (ItemPedido item in carrito.Items)
        {
            Producto? producto = _productoRepo.GetById(item.ProductoId);
            if (producto != null)
                total += producto.Precio * item.Cantidad;
        }

        return total;
    }

    // ReadFilter

    public IList<Carrito> ReadFilter(long? usuarioId = null, bool? conItems = null)
    {
        return _carritoRepo.ReadFilter(usuarioId, conItems).ToList();
    }
}
