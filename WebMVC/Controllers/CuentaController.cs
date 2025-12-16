using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using System.Linq;

namespace WebMVC.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly TarjetaCEN _tarjetaCEN;

        public CuentaController(UsuarioCEN usuarioCEN, TarjetaCEN tarjetaCEN)
        {
            _usuarioCEN = usuarioCEN;
            _tarjetaCEN = tarjetaCEN;
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

        // GET: Cuenta (Menú principal)
        public IActionResult Index()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioCEN.ReadOID(usuarioId);

            return View(usuario);
        }

        // GET: Cuenta/EditarPerfil
        public IActionResult EditarPerfil()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioCEN.ReadOID(usuarioId);

            return View(usuario);
        }

        // POST: Cuenta/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(string nombre, string email, string? telefono, string? passwordActual, string? passwordNueva, string? passwordConfirmar)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            try
            {
                var usuarioId = ObtenerUsuarioId();
                var usuario = _usuarioCEN.ReadOID(usuarioId);

                // Validar email único si cambió
                if (email != usuario.Email)
                {
                    var usuarioExistente = _usuarioCEN.ReadAll().FirstOrDefault(u => u.Email == email && u.Id != usuarioId);
                    if (usuarioExistente != null)
                    {
                        ViewBag.Error = "El email ya está en uso por otro usuario";
                        return View(usuario);
                    }
                }

                // Actualizar datos básicos
                usuario.Nombre = nombre;
                usuario.Email = email;
                usuario.Telefono = telefono;

                // Si quiere cambiar contraseña
                if (!string.IsNullOrEmpty(passwordNueva))
                {
                    if (string.IsNullOrEmpty(passwordActual))
                    {
                        ViewBag.Error = "Debe ingresar la contraseña actual";
                        return View(usuario);
                    }

                    if (passwordNueva != passwordConfirmar)
                    {
                        ViewBag.Error = "Las contraseñas nuevas no coinciden";
                        return View(usuario);
                    }

                    // Verificar contraseña actual
                    try
                    {
                        _usuarioCEN.Login(usuario.Email, passwordActual);
                    }
                    catch
                    {
                        ViewBag.Error = "La contraseña actual es incorrecta";
                        return View(usuario);
                    }

                    usuario.Contrasenya = passwordNueva;
                }

                _usuarioCEN.Modify(usuario);

                // Actualizar sesión si cambió el nombre
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

                TempData["Success"] = "Perfil actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al actualizar el perfil: {ex.Message}";
                var usuarioId = ObtenerUsuarioId();
                var usuario = _usuarioCEN.ReadOID(usuarioId);
                return View(usuario);
            }
        }

        // GET: Cuenta/CerrarSesion
        public IActionResult CerrarSesion()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: Cuenta/CerrarSesion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarCerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // GET: Cuenta/Ayuda
        public IActionResult Ayuda()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // GET: Cuenta/Contacto
        public IActionResult Contacto()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // GET: Cuenta/EliminarCuenta
        public IActionResult EliminarCuenta()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioCEN.ReadOID(usuarioId);

            return View(usuario);
        }

        // POST: Cuenta/EliminarCuenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarEliminarCuenta(string password)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            try
            {
                var usuarioId = ObtenerUsuarioId();
                var usuario = _usuarioCEN.ReadOID(usuarioId);

                // Verificar contraseña antes de eliminar
                try
                {
                    _usuarioCEN.Login(usuario.Email, password);
                }
                catch
                {
                    ViewBag.Error = "La contraseña es incorrecta";
                    return View("EliminarCuenta", usuario);
                }

                // Eliminar usuario
                _usuarioCEN.Destroy(usuarioId);

                // Cerrar sesión
                HttpContext.Session.Clear();

                // Redirigir con mensaje
                TempData["DeleteSuccess"] = "Tu cuenta ha sido eliminada correctamente";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                var usuarioId = ObtenerUsuarioId();
                var usuario = _usuarioCEN.ReadOID(usuarioId);
                ViewBag.Error = $"Error al eliminar la cuenta: {ex.Message}";
                return View("EliminarCuenta", usuario);
            }
        }

        // GET: Cuenta/DatosEnvio
        public IActionResult DatosEnvio()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuario = _usuarioCEN.ReadOID(ObtenerUsuarioId());
            return View(usuario);
        }

        // POST: Cuenta/DatosEnvio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarDatosEnvio(string? nombreEnvio, string? direccionEnvio, string? ciudadEnvio, string? cpEnvio, string? telefonoEnvio)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();

            _usuarioCEN.Modify(usuarioId, nombreEnvio: nombreEnvio, direccionEnvio: direccionEnvio, ciudadEnvio: ciudadEnvio, cpEnvio: cpEnvio, telefonoEnvio: telefonoEnvio);
            TempData["Success"] = "Datos de envío actualizados";
            return RedirectToAction(nameof(DatosEnvio));
        }

        // GET: Cuenta/Tarjetas
        public IActionResult Tarjetas()
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            var tarjetas = _tarjetaCEN.Listar(usuarioId);
            ViewBag.Predeterminada = _tarjetaCEN.ObtenerPredeterminada(usuarioId)?.Id;
            return View(tarjetas);
        }

        // POST: Cuenta/AgregarTarjeta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarTarjeta(string numeroTarjeta, int mesExp, int anioExp, string nombreTitular, bool esPredeterminada)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            string limpio = (numeroTarjeta ?? string.Empty).Replace(" ", "");
            if (string.IsNullOrWhiteSpace(limpio) || limpio.Length < 12)
            {
                TempData["Error"] = "Número de tarjeta inválido";
                return RedirectToAction(nameof(Tarjetas));
            }

            string ultimos4 = limpio.Length >= 4 ? limpio[^4..] : limpio;
            string enmascarado = $"**** **** **** {ultimos4}";
            string marca = limpio.StartsWith("4") ? "VISA" : limpio.StartsWith("5") ? "MASTERCARD" : "TARJETA";

            _tarjetaCEN.Agregar(usuarioId, marca, enmascarado, ultimos4, mesExp, anioExp, nombreTitular, esPredeterminada);
            TempData["Success"] = "Tarjeta guardada";
            return RedirectToAction(nameof(Tarjetas));
        }

        // POST: Cuenta/PredeterminadaTarjeta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PredeterminadaTarjeta(long tarjetaId)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            _tarjetaCEN.EstablecerPredeterminada(usuarioId, tarjetaId);
            TempData["Success"] = "Tarjeta establecida como predeterminada";
            return RedirectToAction(nameof(Tarjetas));
        }

        // POST: Cuenta/EliminarTarjeta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarTarjeta(long tarjetaId)
        {
            if (!EstaLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = ObtenerUsuarioId();
            _tarjetaCEN.Eliminar(usuarioId, tarjetaId);
            TempData["Success"] = "Tarjeta eliminada";
            return RedirectToAction(nameof(Tarjetas));
        }
    }
}
