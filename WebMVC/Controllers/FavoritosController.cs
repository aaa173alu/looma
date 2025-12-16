using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace WebMVC.Controllers
{
    public class FavoritosController : Controller
    {
        private readonly FavoritosCEN _favoritosCEN;
        private readonly ProductoCEN _productoCEN;

        public FavoritosController(FavoritosCEN favoritosCEN, ProductoCEN productoCEN)
        {
            _favoritosCEN = favoritosCEN;
            _productoCEN = productoCEN;
        }

        private bool EstaLogueado()
        {
            return HttpContext.Session.GetString("UsuarioId") != null;
        }

        private long ObtenerUsuarioId()
        {
            var userId = HttpContext.Session.GetString("UsuarioId");
            return long.Parse(userId ?? "0");
        }

        // GET: /Favoritos
        public IActionResult Index()
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioId = ObtenerUsuarioId();
            var favoritos = _favoritosCEN.ObtenerPorUsuario(usuarioId);

            // Cargar información completa de los productos
            var productosConDetalles = new List<Producto>();
            foreach (var fav in favoritos)
            {
                var producto = _productoCEN.ReadOID(fav.ProductoId);
                if (producto != null)
                {
                    productosConDetalles.Add(producto);
                }
            }

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View(productosConDetalles);
        }

        // POST: /Favoritos/Agregar (AJAX)
        [HttpPost]
        public IActionResult Agregar([FromBody] AgregarFavoritoRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debe iniciar sesión" });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                
                // Verificar si ya existe en favoritos
                var favoritosExistentes = _favoritosCEN.ObtenerPorUsuario(usuarioId);
                if (favoritosExistentes.Any(f => f.ProductoId == request.ProductoId))
                {
                    return Json(new { success = false, message = "Ya está en favoritos" });
                }

                // Agregar a favoritos
                _favoritosCEN.Crear(usuarioId, request.ProductoId);

                // Obtener nuevo contador
                var nuevoContador = _favoritosCEN.ObtenerPorUsuario(usuarioId).Count;

                return Json(new { success = true, message = "Agregado a favoritos", count = nuevoContador });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar a favoritos: " + ex.Message });
            }
        }

        // POST: /Favoritos/Eliminar
        [HttpPost]
        public IActionResult Eliminar(long productoId)
        {
            if (!EstaLogueado())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                
                // Buscar el favorito
                var favoritos = _favoritosCEN.ObtenerPorUsuario(usuarioId);
                var favorito = favoritos.FirstOrDefault(f => f.ProductoId == productoId);

                if (favorito != null)
                {
                    _favoritosCEN.Destroy(favorito.Id);
                    TempData["Success"] = "Producto eliminado de favoritos";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar de favoritos: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: /Favoritos/EliminarAjax (AJAX)
        [HttpPost]
        public IActionResult EliminarAjax([FromBody] EliminarFavoritoRequest request)
        {
            if (!EstaLogueado())
            {
                return Json(new { success = false, message = "Debe iniciar sesión" });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                
                // Buscar el favorito
                var favoritos = _favoritosCEN.ObtenerPorUsuario(usuarioId);
                var favorito = favoritos.FirstOrDefault(f => f.ProductoId == request.ProductoId);

                if (favorito != null)
                {
                    _favoritosCEN.Destroy(favorito.Id);
                    
                    // Obtener nuevo contador
                    var nuevoContador = _favoritosCEN.ObtenerPorUsuario(usuarioId).Count;
                    
                    return Json(new { success = true, message = "Eliminado de favoritos", count = nuevoContador });
                }

                return Json(new { success = false, message = "Favorito no encontrado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        // GET: /Favoritos/Contador (AJAX)
        public IActionResult Contador()
        {
            if (!EstaLogueado())
            {
                return Json(new { count = 0 });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                var favoritos = _favoritosCEN.ObtenerPorUsuario(usuarioId);
                return Json(new { count = favoritos.Count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        // GET: /Favoritos/VerificarFavorito (AJAX)
        public IActionResult VerificarFavorito(long productoId)
        {
            if (!EstaLogueado())
            {
                return Json(new { esFavorito = false });
            }

            try
            {
                var usuarioId = ObtenerUsuarioId();
                var favoritos = _favoritosCEN.ObtenerPorUsuario(usuarioId);
                var esFavorito = favoritos.Any(f => f.ProductoId == productoId);
                
                return Json(new { esFavorito = esFavorito });
            }
            catch
            {
                return Json(new { esFavorito = false });
            }
        }
    }

    // Clases de request para AJAX
    public class AgregarFavoritoRequest
    {
        public long ProductoId { get; set; }
    }

    public class EliminarFavoritoRequest
    {
        public long ProductoId { get; set; }
    }
}
