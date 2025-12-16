using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using Infrastructure.NHibernate;

namespace WebMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsuarioCEN _usuarioCEN;

        public AccountController(UsuarioCEN usuarioCEN)
        {
            _usuarioCEN = usuarioCEN;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var usuario = _usuarioCEN.Login(email, password);
                HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "Cliente");
                
                // Redirigir según el rol
                if (usuario.Rol?.ToLower() == "admin")
                {
                    return RedirectToAction("Index", "Admin"); // Panel de administración
                }
                else
                {
                    return RedirectToAction("Index", "Tienda"); // Tienda para clientes
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
                ViewBag.Error = "Credenciales inválidas";
                return View();
            }
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string nombre, string email, string password, string passwordConfirm)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Completa todos los campos";
                return View();
            }

            if (password != passwordConfirm)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View();
            }

            try
            {
                var usuario = _usuarioCEN.Registrar(nombre, email, password);

                HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "Cliente");

                return RedirectToAction("Index", "Tienda");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("IndexPublico", "Tienda");
        }
    }
}
