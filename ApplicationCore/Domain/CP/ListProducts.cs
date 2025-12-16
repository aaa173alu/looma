using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.CP;

public class ListProducts
{
    private readonly ProductoCEN _productoCEN;

    public ListProducts(ProductoCEN productoCEN)
    {
        _productoCEN = productoCEN;
    }

    public IEnumerable<Producto> Execute() => _productoCEN.ListarTodos();
}
