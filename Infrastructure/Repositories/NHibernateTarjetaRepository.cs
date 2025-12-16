using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateTarjetaRepository : ITarjetaRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateTarjetaRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Tarjeta New(Tarjeta entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Tarjeta entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Tarjeta e = _uow.Session.Get<Tarjeta>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Tarjeta? GetById(long id) => _uow.Session.Get<Tarjeta>(id);

    public IEnumerable<Tarjeta> GetAll() => _uow.Session.Query<Tarjeta>().ToList();

    public IEnumerable<Tarjeta> GetByUsuario(long usuarioId)
    {
        return _uow.Session.Query<Tarjeta>().Where(t => t.Usuario.Id == usuarioId).OrderByDescending(t => t.EsPredeterminada).ThenByDescending(t => t.FechaAlta).ToList();
    }

    public Tarjeta? GetPredeterminada(long usuarioId)
    {
        return _uow.Session.Query<Tarjeta>().FirstOrDefault(t => t.Usuario.Id == usuarioId && t.EsPredeterminada);
    }

    public void LimpiarPredeterminada(long usuarioId)
    {
        var tarjetas = _uow.Session.Query<Tarjeta>().Where(t => t.Usuario.Id == usuarioId && t.EsPredeterminada).ToList();
        foreach (var t in tarjetas)
        {
            t.EsPredeterminada = false;
            _uow.Session.Update(t);
        }
    }
}
