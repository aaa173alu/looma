using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories;

public class NHibernateUsuarioRepository : IUsuarioRepository
{
    private readonly NHibernateUnitOfWork _uow;

    public NHibernateUsuarioRepository(NHibernateUnitOfWork uow)
    {
        _uow = uow;
    }

    public Usuario New(Usuario entity)
    {
        _uow.Session.Save(entity);
        return entity;
    }

    public void Modify(Usuario entity) => _uow.Session.Update(entity);

    public void Destroy(long id)
    {
        Usuario e = _uow.Session.Get<Usuario>(id);
        if (e != null) _uow.Session.Delete(e);
    }

    public Usuario? GetById(long id) => _uow.Session.Get<Usuario>(id);

    public IEnumerable<Usuario> GetAll() => _uow.Session.Query<Usuario>().ToList();

    public IEnumerable<Usuario> ReadFilter(string nombre = null, string email = null, string telefono = null)
    {
        string hql = "from Usuario u where 1=1";
        if (!string.IsNullOrEmpty(nombre)) hql += " and u.Nombre like :nombre";
        if (!string.IsNullOrEmpty(email)) hql += " and u.Email like :email";
        if (!string.IsNullOrEmpty(telefono)) hql += " and u.Telefono like :telefono";

        global::NHibernate.IQuery q = _uow.Session.CreateQuery(hql);
        if (!string.IsNullOrEmpty(nombre)) q.SetParameter("nombre", "%" + nombre + "%");
        if (!string.IsNullOrEmpty(email)) q.SetParameter("email", "%" + email + "%");
        if (!string.IsNullOrEmpty(telefono)) q.SetParameter("telefono", "%" + telefono + "%");
        return q.List<Usuario>();
    }
}
