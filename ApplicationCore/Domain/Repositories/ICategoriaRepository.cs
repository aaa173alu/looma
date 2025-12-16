using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories;

public interface ICategoriaRepository : IRepository<Categoria, long>
{
    IEnumerable<Categoria> ReadFilter(string nombre = null);
}
