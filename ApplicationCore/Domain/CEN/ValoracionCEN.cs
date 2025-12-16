using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CEN;

public class ValoracionCEN
{
    private readonly IValoracionRepository _valoracionRepo;
    private readonly IUnitOfWork _uow;

    public ValoracionCEN(IValoracionRepository valoracionRepo, IUnitOfWork uow)
    {
        _valoracionRepo = valoracionRepo;
        _uow = uow;
    }

    // CRUD Básico

    public Valoracion Crear(long usuarioId, long productoId, int valor, string comentario = null)
    {
        if (valor < 1 || valor > 5)
            throw new Exception("El valor debe estar entre 1 y 5");

        Valoracion valoracion = new Valoracion
        {
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Valor = valor,
            Comentario = comentario
        };

        Valoracion created = _valoracionRepo.New(valoracion);
        _uow.SaveChanges();
        return created;
    }

    public void Modify(long id, int? valor = null, string comentario = null)
    {
        Valoracion? valoracion = _valoracionRepo.GetById(id);
        if (valoracion == null)
            throw new Exception($"Valoración con ID {id} no encontrada");

        if (valor.HasValue)
        {
            if (valor.Value < 1 || valor.Value > 5)
                throw new Exception("El valor debe estar entre 1 y 5");
            valoracion.Valor = valor.Value;
        }

        if (comentario != null)
            valoracion.Comentario = comentario;

        _valoracionRepo.Modify(valoracion);
        _uow.SaveChanges();
    }

    public void Destroy(long id)
    {
        _valoracionRepo.Destroy(id);
        _uow.SaveChanges();
    }

    public Valoracion ReadOID(long id)
    {
        return _valoracionRepo.GetById(id);
    }

    public IList<Valoracion> ReadAll()
    {
        return _valoracionRepo.GetAll().ToList();
    }

    // Operaciones Custom

    public IList<Valoracion> ObtenerPorProducto(long productoId)
    {
        return _valoracionRepo.GetAll()
            .Where(v => v.ProductoId == productoId)
            .ToList();
    }

    public IList<Valoracion> ObtenerPorUsuario(long usuarioId)
    {
        return _valoracionRepo.GetAll()
            .Where(v => v.UsuarioId == usuarioId)
            .ToList();
    }

    public double CalcularPromedioProducto(long productoId)
    {
        List<Valoracion> valoraciones = _valoracionRepo.GetAll()
            .Where(v => v.ProductoId == productoId)
            .ToList();

        if (!valoraciones.Any())
            return 0;

        return valoraciones.Average(v => v.Valor);
    }

    // ReadFilter

    public IList<Valoracion> ReadFilter(
        long? usuarioId = null,
        long? productoId = null,
        int? valorMin = null,
        int? valorMax = null)
    {
        return _valoracionRepo.ReadFilter(usuarioId, productoId, valorMin, valorMax).ToList();
    }
}
