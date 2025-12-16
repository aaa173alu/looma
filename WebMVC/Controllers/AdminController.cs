using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace WebMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly PedidoCEN _pedidoCEN;
        private readonly UsuarioCEN _usuarioCEN;

        public AdminController(PedidoCEN pedidoCEN, UsuarioCEN usuarioCEN)
        {
            _pedidoCEN = pedidoCEN;
            _usuarioCEN = usuarioCEN;
        }

        private bool EstaLogueado() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));

        private long ObtenerUsuarioId()
        {
            var userId = HttpContext.Session.GetString("UsuarioId");
            return string.IsNullOrEmpty(userId) ? 0 : long.Parse(userId);
        }

        private bool EsAdmin()
        {
            if (!EstaLogueado()) return false;
            var usuario = _usuarioCEN.ReadOID(ObtenerUsuarioId());
            return usuario?.Rol?.ToLower() == "admin";
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            // Obtener estadísticas
            var pedidos = _pedidoCEN.ReadAll();
            var usuarios = _usuarioCEN.ReadAll();

            // Pedidos completados (comprado, validado, recibido)
            var pedidosCompletados = pedidos
                .Where(p => p.Estado == EstadoPedido.comprado || 
                           p.Estado == EstadoPedido.validado || 
                           p.Estado == EstadoPedido.recibido)
                .ToList();

            var stats = new DashboardStatsViewModel
            {
                TotalVentas = pedidosCompletados.Sum(p => p.Total),
                NumPedidos = pedidosCompletados.Count,
                NumUsuarios = usuarios.Count,
                PedidosPendientes = pedidos.Count(p => p.Estado == EstadoPedido.comprado),
                PedidosEnviados = pedidos.Count(p => p.Estado == EstadoPedido.validado),
                PedidosEntregados = pedidos.Count(p => p.Estado == EstadoPedido.recibido),
                VentasHoy = pedidosCompletados
                    .Where(p => p.Fecha.Date == System.DateTime.Today)
                    .Sum(p => p.Total),
                VentasMes = pedidosCompletados
                    .Where(p => p.Fecha.Month == System.DateTime.Now.Month && 
                               p.Fecha.Year == System.DateTime.Now.Year)
                    .Sum(p => p.Total)
            };

            return View(stats);
        }

        [HttpGet]
        public IActionResult Reportes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var pedidos = _pedidoCEN.ReadAll()
                .Where(p => p.Estado != EstadoPedido.carrito && p.Estado != EstadoPedido.realizado)
                .ToList();

            var reportes = new ReportesViewModel
            {
                VentasPorMes = pedidos
                    .GroupBy(p => new { p.Fecha.Year, p.Fecha.Month })
                    .Select(g => new VentaMensual
                    {
                        Año = g.Key.Year,
                        Mes = g.Key.Month,
                        Total = g.Sum(p => p.Total),
                        NumPedidos = g.Count()
                    })
                    .OrderByDescending(v => v.Año)
                    .ThenByDescending(v => v.Mes)
                    .Take(6)
                    .ToList()
            };

            return View(reportes);
        }

        [HttpGet]
        public IActionResult Pedidos()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var pedidos = _pedidoCEN.ReadAll()
                .Where(p => p.Estado != EstadoPedido.realizado && p.Estado != EstadoPedido.carrito)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            var vm = pedidos.Select(p => new AdminPedidoViewModel
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                Fecha = p.Fecha,
                Total = p.Total,
                Direccion = p.DireccionEnvio,
                Estado = p.Estado
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public IActionResult CambiarEstado([FromBody] CambiarEstadoRequest request)
        {
            if (!EsAdmin())
            {
                return Json(new { success = false, message = "No autorizado" });
            }

            try
            {
                _pedidoCEN.CambiarEstado(request.PedidoId, request.Estado);
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Usuarios(string buscar = "")
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var usuarios = _usuarioCEN.ReadAll();

            // Filtrar por búsqueda
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                usuarios = usuarios
                    .Where(u => u.Nombre.ToLower().Contains(buscar.ToLower()) ||
                                u.Email.ToLower().Contains(buscar.ToLower()))
                    .ToList();
            }

            var vm = usuarios.Select(u => new AdminUsuarioViewModel
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono ?? string.Empty,
                Rol = u.Rol ?? "Cliente",
                NumPedidos = _pedidoCEN.ReadAll().Count(p => p.UsuarioId == u.Id && 
                    p.Estado != EstadoPedido.carrito && p.Estado != EstadoPedido.realizado)
            }).OrderBy(u => u.Nombre).ToList();

            ViewBag.BusquedaTerm = buscar;
            return View(vm);
        }

        [HttpPost]
        public IActionResult CambiarRol([FromBody] CambiarRolRequest request)
        {
            if (!EsAdmin())
            {
                return Json(new { success = false, message = "No autorizado" });
            }

            try
            {
                var usuario = _usuarioCEN.ReadOID(request.UsuarioId);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // No permitir que se elimine el rol admin del último admin
                if (usuario.Rol?.ToLower() == "admin" && request.NuevoRol.ToLower() != "admin")
                {
                    var otrosAdmins = _usuarioCEN.ReadAll().Count(u => u.Id != request.UsuarioId && u.Rol?.ToLower() == "admin");
                    if (otrosAdmins == 0)
                    {
                        return Json(new { success = false, message = "No puedes eliminar el rol admin del único administrador" });
                    }
                }

                // Cambiar el rol del usuario
                usuario.Rol = request.NuevoRol;
                _usuarioCEN.Modify(usuario);
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class AdminPedidoViewModel
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public System.DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public EstadoPedido Estado { get; set; }
    }

    public class CambiarEstadoRequest
    {
        public long PedidoId { get; set; }
        public EstadoPedido Estado { get; set; }
    }

    public class AdminUsuarioViewModel
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cliente";
        public int NumPedidos { get; set; }
    }

    public class CambiarRolRequest
    {
        public long UsuarioId { get; set; }
        public string NuevoRol { get; set; } = string.Empty;
    }

    public class DashboardStatsViewModel
    {
        public decimal TotalVentas { get; set; }
        public int NumPedidos { get; set; }
        public int NumUsuarios { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosEnviados { get; set; }
        public int PedidosEntregados { get; set; }
        public decimal VentasHoy { get; set; }
        public decimal VentasMes { get; set; }
    }

    public class ReportesViewModel
    {
        public List<VentaMensual> VentasPorMes { get; set; } = new List<VentaMensual>();
    }

    public class VentaMensual
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public decimal Total { get; set; }
        public int NumPedidos { get; set; }
        
        public string NombreMes => new System.DateTime(Año, Mes, 1).ToString("MMMM yyyy");
    }
}
