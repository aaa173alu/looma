using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.CP;

public class CreateProduct
{
    private readonly ProductoCEN _productoCEN;

    public CreateProduct(ProductoCEN productoCEN)
    {
        _productoCEN = productoCEN;
    }

    public Producto Execute(string nombre, decimal precio, int stock, bool destacado)
    {
        return _productoCEN.Crear(nombre, precio, stock, destacado);
    }
}
