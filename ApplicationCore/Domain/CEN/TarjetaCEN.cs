using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Domain.CEN;

public class TarjetaCEN
{
    private readonly ITarjetaRepository _tarjetaRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IUnitOfWork _uow;

    public TarjetaCEN(ITarjetaRepository tarjetaRepo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
    {
        _tarjetaRepo = tarjetaRepo;
        _usuarioRepo = usuarioRepo;
        _uow = uow;
    }

    public Tarjeta Agregar(long usuarioId, string marca, string numeroEnmascarado, string ultimos4, int mesExp, int anioExp, string titular, bool esPredeterminada)
    {
        Usuario? usuario = _usuarioRepo.GetById(usuarioId);
        if (usuario == null) throw new Exception("Usuario no encontrado");

        if (esPredeterminada)
        {
            _tarjetaRepo.LimpiarPredeterminada(usuarioId);
        }

        var tarjeta = new Tarjeta
        {
            Usuario = usuario,
            Marca = marca,
            NumeroEnmascarado = numeroEnmascarado,
            Ultimos4 = ultimos4,
            MesExp = mesExp,
            AnioExp = anioExp,
            NombreTitular = titular,
            EsPredeterminada = esPredeterminada,
            FechaAlta = DateTime.UtcNow
        };

        var created = _tarjetaRepo.New(tarjeta);
        _uow.SaveChanges();
        return created;
    }

    public void EstablecerPredeterminada(long usuarioId, long tarjetaId)
    {
        Tarjeta? tarjeta = _tarjetaRepo.GetById(tarjetaId);
        if (tarjeta == null || tarjeta.Usuario.Id != usuarioId)
            throw new Exception("Tarjeta no encontrada o no pertenece al usuario");

        _tarjetaRepo.LimpiarPredeterminada(usuarioId);
        tarjeta.EsPredeterminada = true;
        _tarjetaRepo.Modify(tarjeta);
        _uow.SaveChanges();
    }

    public void Eliminar(long usuarioId, long tarjetaId)
    {
        Tarjeta? tarjeta = _tarjetaRepo.GetById(tarjetaId);
        if (tarjeta == null || tarjeta.Usuario.Id != usuarioId)
            throw new Exception("Tarjeta no encontrada o no pertenece al usuario");

        _tarjetaRepo.Destroy(tarjetaId);
        _uow.SaveChanges();
    }

    public IList<Tarjeta> Listar(long usuarioId)
    {
        return _tarjetaRepo.GetByUsuario(usuarioId).ToList();
    }

    public Tarjeta? ObtenerPredeterminada(long usuarioId)
    {
        return _tarjetaRepo.GetPredeterminada(usuarioId);
    }
}
