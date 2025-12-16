using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CEN;

public class CategoriaCEN
{
    private readonly ICategoriaRepository _categoriaRepo;
    private readonly IUnitOfWork _uow;

    public CategoriaCEN(ICategoriaRepository categoriaRepo, IUnitOfWork uow)
    {
        _categoriaRepo = categoriaRepo;
        _uow = uow;
    }

    // CRUD Básico

    public Categoria Crear(string nombre)
    {
        // Verificar nombre único
        Categoria? existente = _categoriaRepo.GetAll().FirstOrDefault(c => c.Nombre == nombre);
        if (existente != null)
            throw new Exception("Ya existe una categoría con ese nombre");

        Categoria categoria = new Categoria { Nombre = nombre };
        Categoria created = _categoriaRepo.New(categoria);
        _uow.SaveChanges();
        return created;
    }

    public void Modify(long id, string nombre)
    {
        Categoria? categoria = _categoriaRepo.GetById(id);
        if (categoria == null)
            throw new Exception($"Categoría con ID {id} no encontrada");

        categoria.Nombre = nombre;
        _categoriaRepo.Modify(categoria);
        _uow.SaveChanges();
    }

    public void Destroy(long id)
    {
        _categoriaRepo.Destroy(id);
        _uow.SaveChanges();
    }

    public Categoria ReadOID(long id)
    {
        return _categoriaRepo.GetById(id);
    }

    public IList<Categoria> ReadAll()
    {
        return _categoriaRepo.GetAll().ToList();
    }

    // Operaciones Custom

    public Categoria BuscarPorNombre(string nombre)
    {
        return _categoriaRepo.GetAll().FirstOrDefault(c => c.Nombre == nombre);
    }

    // ReadFilter

    public IList<Categoria> ReadFilter(string nombre = null)
    {
        return _categoriaRepo.ReadFilter(nombre).ToList();
    }
}
