using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CEN;

public class ProductoCEN
{
    private readonly IProductoRepository _productoRepo;
    private readonly IUnitOfWork _uow;

    public ProductoCEN(IProductoRepository productoRepo, IUnitOfWork uow)
    {
        _productoRepo = productoRepo;
        _uow = uow;
    }

    // CRUD Básico

    public Producto Crear(string nombre, decimal precio, int stock, bool destacado, IList<string>? fotos = null)
    {
        System.Diagnostics.Debug.WriteLine("=== CREAR PRODUCTO INICIADO ===");
        var p = new Producto
        {
            Nombre = nombre,
            Precio = precio,
            Stock = stock,
            Destacado = destacado,
            Fotos = fotos?.ToList()
        };
        System.Diagnostics.Debug.WriteLine($"Memoria -> Nombre:{p.Nombre} Precio:{p.Precio} Stock:{p.Stock}");
        var created = _productoRepo.New(p);
        System.Diagnostics.Debug.WriteLine($"Tras New() Id:{created.Id}");
        try
        {
            _uow.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"Tras SaveChanges() Id:{created.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("EXCEPCIÓN en SaveChanges: " + ex);
            throw;
        }
        return created;
    }

    public void Modify(long id, string nombre = null, decimal? precio = null, int? stock = null, bool? destacado = null, string descripcion = null, IList<string>? fotos = null)
    {
        Producto? producto = _productoRepo.GetById(id);
        if (producto == null)
            throw new Exception($"Producto con ID {id} no encontrado");

        if (!string.IsNullOrEmpty(nombre))
            producto.Nombre = nombre;
        if (precio.HasValue)
            producto.Precio = precio.Value;
        if (stock.HasValue)
            producto.Stock = stock.Value;
        if (destacado.HasValue)
            producto.Destacado = destacado.Value;
        if (descripcion != null)
            producto.Descripcion = descripcion;
        if (fotos != null)
            producto.Fotos = fotos.ToList();

        _productoRepo.Modify(producto);
        _uow.SaveChanges();
    }

    public void Destroy(long id)
    {
        _productoRepo.Destroy(id);
        _uow.SaveChanges();
    }

    public Producto ReadOID(long id)
    {
        return _productoRepo.GetById(id);
    }

    public IEnumerable<Producto> ReadAll() => _productoRepo.GetAll();

    public IList<Producto> ListarTodos() => _productoRepo.GetAll().ToList();

    // Operaciones Custom

    public IList<Producto> BuscarPorRangoPrecio(decimal precioMin, decimal precioMax)
    {
        return _productoRepo.GetAll()
            .Where(p => p.Precio >= precioMin && p.Precio <= precioMax)
            .ToList();
    }

    public IList<Producto> ObtenerDestacados()
    {
        return _productoRepo.GetAll()
            .Where(p => p.Destacado)
            .ToList();
    }

    public void IncrementarStock(long id, int cantidad)
    {
        Producto? producto = _productoRepo.GetById(id);
        if (producto == null)
            throw new Exception($"Producto con ID {id} no encontrado");

        producto.Stock += cantidad;
        _productoRepo.Modify(producto);
        _uow.SaveChanges();
    }

    public void DecrementarStock(long id, int cantidad)
    {
        Producto? producto = _productoRepo.GetById(id);
        if (producto == null)
            throw new Exception($"Producto con ID {id} no encontrado");

        if (producto.Stock < cantidad)
            throw new Exception("Stock insuficiente");

        producto.Stock -= cantidad;
        _productoRepo.Modify(producto);
        _uow.SaveChanges();
    }

    public void ActualizarColor(long id, string color)
    {
        Producto? producto = _productoRepo.GetById(id);
        if (producto == null)
            throw new Exception($"Producto con ID {id} no encontrado");

        producto.Color = color;
        _productoRepo.Modify(producto);
        _uow.SaveChanges();
    }

    // ReadFilter
    public IList<Producto> ReadFilter(
        decimal? precioMin = null,
        decimal? precioMax = null,
        int? stockMin = null,
        bool? destacado = null,
        string nombre = null,
        string color = null)
    {
        return _productoRepo.ReadFilter(precioMin, precioMax, stockMin, destacado, nombre, color).ToList();
    }
}
