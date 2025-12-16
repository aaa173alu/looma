using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario, long>
{
    IEnumerable<Usuario> ReadFilter(string nombre = null, string email = null, string telefono = null);
}
