using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;

namespace WebMVC.Controllers
{
    public class CarritoController : Controller
    {
        private readonly CarritoCEN _carritoCEN;
        private readonly ProductoCEN _productoCEN;
        private readonly AgregarProductoAlCarritoCP _agregarProductoCP;

        public CarritoController(CarritoCEN carritoCEN, ProductoCEN productoCEN, AgregarProductoAlCarritoCP agregarProductoCP)
        {
            _carritoCEN = carritoCEN;
            _productoCEN = productoCEN;
            _agregarProductoCP = agregarProductoCP;
        }

        // Verificar que el usuario esté logueado
        private bool EstaLogueado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        private long ObtenerUsuarioId()
        {
            var userId = HttpContext.Session.GetString("UsuarioId");
            return string.IsNullOrEmpty(userId) ? 0 : long.Parse(userId);
        }

        private long ObtenerOCrearCarritoId()
        {
            var usuarioId = ObtenerUsuarioId();
            
            // Buscar carrito existente del usuario en la base de datos
            var carritoExistente = _carritoCEN.ObtenerPorUsuario(usuarioId);
            
            if (carritoExistente != null)
            {
                // Guardar en sesión para acceso rápido
                HttpContext.Session.SetString("CarritoId", carritoExistente.Id.ToString());
                return carritoExistente.Id;
            }

            // Si no existe, crear carrito nuevo para el usuario
            var carrito = _carritoCEN.Crear(usuarioId);
            HttpContext.Session.SetString("CarritoId", carrito.Id.ToString());
            return carrito.Id;
        }

        // GET: Carrito
        public IActionResult Index()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            var carritoId = ObtenerOCrearCarritoId();
            var carrito = _carritoCEN.ReadOID(carritoId);
            
            if (carrito == null)
            {
                // Si no hay carrito, crear uno nuevo
                var usuarioId = ObtenerUsuarioId();
                carrito = _carritoCEN.Crear(usuarioId);
                HttpContext.Session.SetString("CarritoId", carrito.Id.ToString());
            }

            // Cargar detalles de productos para mostrar
            var itemsConDetalles = new List<(ItemPedido Item, Producto Producto)>();
            foreach (var item in carrito.Items)
            {
                var producto = _productoCEN.ReadOID(item.ProductoId);
                if (producto != null)
                {
                    itemsConDetalles.Add((item, producto));
                }
            }

            ViewBag.ItemsConDetalles = itemsConDetalles;
            ViewBag.Total = _carritoCEN.CalcularTotal(carritoId);

            return View(carrito);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        public IActionResult Agregar([FromBody] AgregarProductoRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            try
            {
                var carritoId = ObtenerOCrearCarritoId();
                _agregarProductoCP.Ejecutar(carritoId, request.ProductoId, request.Cantidad);
                
                var carrito = _carritoCEN.ReadOID(carritoId);
                var itemsCount = carrito?.Items?.Count ?? 0;
                return Json(new { success = true, itemsCount = itemsCount });
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error en Carrito/Agregar: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public class AgregarProductoRequest
        {
            public long ProductoId { get; set; }
            public int Cantidad { get; set; }
            public string? Talla { get; set; }
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        public IActionResult ActualizarCantidad([FromBody] ActualizarCantidadRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "No autenticado" });
            }

            try
            {
                var carritoId = ObtenerOCrearCarritoId();
                _carritoCEN.ActualizarCantidad(carritoId, request.ProductoId, request.Cantidad);
                
                var total = _carritoCEN.CalcularTotal(carritoId);
                return Json(new { success = true, total = total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ActualizarCantidadRequest
        {
            public long ProductoId { get; set; }
            public int Cantidad { get; set; }
        }

        // POST: Carrito/Eliminar
        [HttpPost]
        public IActionResult Eliminar(long productoId)
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var carritoId = ObtenerOCrearCarritoId();
                _carritoCEN.EliminarItem(carritoId, productoId);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        public IActionResult Vaciar()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var carritoId = ObtenerOCrearCarritoId();
                _carritoCEN.Vaciar(carritoId);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al vaciar carrito: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Carrito/Contador (para AJAX)
        public IActionResult Contador()
        {
            if (!EstaLogueado())
            {
                return Json(new { count = 0 });
            }

            try
            {
                var carritoIdStr = HttpContext.Session.GetString("CarritoId");
                if (string.IsNullOrEmpty(carritoIdStr))
                {
                    return Json(new { count = 0 });
                }

                var carritoId = long.Parse(carritoIdStr);
                var carrito = _carritoCEN.ReadOID(carritoId);
                var count = carrito?.Items?.Count ?? 0;
                
                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
    }
}
