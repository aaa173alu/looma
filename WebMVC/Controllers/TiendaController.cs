using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;

namespace WebMVC.Controllers
{
    public class TiendaController : Controller
    {
        private readonly ProductoCEN _productoCEN;

        public TiendaController(ProductoCEN productoCEN)
        {
            _productoCEN = productoCEN;
        }

        // GET: Tienda (Catálogo de productos para clientes)
        public IActionResult Index(string? nombre, decimal? precioMin, decimal? precioMax, bool? destacado, string? color)
        {
            // Verificar que el usuario esté logueado
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            var productos = _productoCEN.ReadFilter(
                precioMin: precioMin,
                precioMax: precioMax,
                destacado: destacado,
                nombre: nombre,
                color: color
            );
            
            // Obtener el producto más nuevo (último ID) para mostrar como novedad
            var todosProductos = _productoCEN.ReadAll();
            var productoNovedad = todosProductos.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.ProductoNovedad = productoNovedad;
            
            ViewBag.Nombre = nombre;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Destacado = destacado;
            ViewBag.Color = color;

            return View(productos);
        }

        // GET: Tienda/Detalle/5
        public IActionResult Detalle(long id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            var producto = _productoCEN.ReadOID(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}
