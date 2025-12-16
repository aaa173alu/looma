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

        // GET: Tienda/IndexPublico (Catálogo público sin login requerido)
        public IActionResult IndexPublico(string? nombre, decimal? precioMin, decimal? precioMax, bool? destacado, string? color, int page = 1)
        {
            const int pageSize = 12;

            var productosFiltrados = _productoCEN.ReadFilter(
                precioMin: precioMin,
                precioMax: precioMax,
                destacado: destacado,
                nombre: nombre,
                color: color
            ).ToList();

            var totalItems = productosFiltrados.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            totalPages = totalPages == 0 ? 1 : totalPages;
            page = Math.Clamp(page, 1, totalPages);
            var productos = productosFiltrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var todosProductos = _productoCEN.ReadAll();
            var productoNovedad = todosProductos.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.ProductoNovedad = productoNovedad;
            
            ViewBag.Nombre = nombre;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Destacado = destacado;
            ViewBag.Color = color;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.EsPublico = true;

            return View("Index", productos);
        }

        // GET: Tienda (Catálogo de productos para clientes autenticados)
        public IActionResult Index(string? nombre, decimal? precioMin, decimal? precioMax, bool? destacado, string? color, int page = 1)
        {
            // Verificar que el usuario esté logueado
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("IndexPublico");
            }

            const int pageSize = 12;

            var productosFiltrados = _productoCEN.ReadFilter(
                precioMin: precioMin,
                precioMax: precioMax,
                destacado: destacado,
                nombre: nombre,
                color: color
            ).ToList();

            var totalItems = productosFiltrados.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            totalPages = totalPages == 0 ? 1 : totalPages;
            page = Math.Clamp(page, 1, totalPages);
            var productos = productosFiltrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var todosProductos = _productoCEN.ReadAll();
            var productoNovedad = todosProductos.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.ProductoNovedad = productoNovedad;
            
            ViewBag.Nombre = nombre;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Destacado = destacado;
            ViewBag.Color = color;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(productos);
        }

        // GET: Tienda/Detalle/5
        public IActionResult Detalle(long id)
        {
            // Requiere login para ver detalles
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
