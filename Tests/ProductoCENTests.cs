using ApplicationCore.Domain.CEN;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using Xunit;

public class ProductoCENTests
{
    [Fact]
    public void CrearYListarProductos_Works()
    {
        InMemoryProductoRepository repo = new InMemoryProductoRepository();
        InMemoryUnitOfWork uow = new InMemoryUnitOfWork();
        ProductoCEN cen = new ProductoCEN(repo, uow);

        ApplicationCore.Domain.EN.Producto p = cen.Crear("TestProd", 10m, 5, false);
        System.Collections.Generic.IList<ApplicationCore.Domain.EN.Producto> list = cen.ListarTodos();

        Assert.Single(list);
        Assert.Equal("TestProd", p.Nombre);
    }
}
