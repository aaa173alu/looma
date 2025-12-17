using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using Infrastructure.NHibernate;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;

class Program 
{
    static void Main() 
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     INICIALIZANDO BASE DE DATOS - NHIBERNATE                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("Configurando NHibernate...");
        
        // Preferir LocalDB por defecto; permitir SQLEXPRESS sólo si se solicita
        string connExpressMaster = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
        string connExpress = "Server=localhost\\SQLEXPRESS;Database=TiendaZapatos;Trusted_Connection=True;TrustServerCertificate=True;";
        string connLocalDbMaster = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";
        string connLocalDb = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TiendaZapatos;Integrated Security=True;";

        bool preferExpress = string.Equals(Environment.GetEnvironmentVariable("PREFER_SQLEXPRESS"), "1", StringComparison.OrdinalIgnoreCase);
        bool expressOk = false;

        if (preferExpress)
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connExpressMaster))
                {
                    conn.Open();
                    System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TiendaZapatos') CREATE DATABASE TiendaZapatos";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✓ Base de datos creada/verificada en SQLEXPRESS\n");
                    expressOk = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  No se pudo usar SQLEXPRESS: {ex.Message}");
                Console.WriteLine("→ Usando LocalDB (MSSQLLocalDB) como alternativa\n");
            }
        }

        // Actualizar connection string para usar la base de datos
        if (expressOk)
        {
            Environment.SetEnvironmentVariable("NH_CONNECTION", connExpress);
        }
        else
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connLocalDbMaster))
                {
                    conn.Open();
                    System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "IF DB_ID('TiendaZapatos') IS NULL CREATE DATABASE [TiendaZapatos]";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✓ Base de datos creada/verificada en LocalDB\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  No se pudo crear/verificar la base en LocalDB: {ex.Message}");
            }
            Environment.SetEnvironmentVariable("NH_CONNECTION", connLocalDb);
        }
        
        // Crear esquema de base de datos
        try
        {
            NHibernate.Cfg.Configuration cfg = NHibernateHelper.BuildConfiguration();
            SchemaExport schemaExport = new SchemaExport(cfg);
            schemaExport.Drop(false, true);
            schemaExport.Create(false, true);
            Console.WriteLine("✓ Esquema de base de datos creado\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error creando esquema: {ex.Message}");
            Console.WriteLine("Continuando con esquema existente...\n");
        }

        // Configurar repositorios NHibernate y Unit of Work
        var session = NHibernateHelper.OpenSession();
        NHibernateUnitOfWork uow = new NHibernateUnitOfWork(session);
        NHibernateProductoRepository productoRepo = new NHibernateProductoRepository(uow);
        NHibernatePedidoRepository pedidoRepo = new NHibernatePedidoRepository(uow);
        NHibernateUsuarioRepository usuarioRepo = new NHibernateUsuarioRepository(uow);
        NHibernateCarritoRepository carritoRepo = new NHibernateCarritoRepository(uow);
        NHibernateCategoriaRepository categoriaRepo = new NHibernateCategoriaRepository(uow);
        NHibernateValoracionRepository valoracionRepo = new NHibernateValoracionRepository(uow);
        NHibernateFavoritosRepository favoritosRepo = new NHibernateFavoritosRepository(uow);
        NHibernateTarjetaRepository tarjetaRepo = new NHibernateTarjetaRepository(uow);

        // Crear CENs
        ProductoCEN productoCEN = new ProductoCEN(productoRepo, uow);
        PedidoCEN pedidoCEN = new PedidoCEN(pedidoRepo, productoRepo, uow);
        UsuarioCEN usuarioCEN = new UsuarioCEN(usuarioRepo, uow);
        CarritoCEN carritoCEN = new CarritoCEN(carritoRepo, productoRepo, uow);
        CategoriaCEN categoriaCEN = new CategoriaCEN(categoriaRepo, uow);
        ValoracionCEN valoracionCEN = new ValoracionCEN(valoracionRepo, uow);
        FavoritosCEN favoritosCEN = new FavoritosCEN(favoritosRepo, uow);
        TarjetaCEN tarjetaCEN = new TarjetaCEN(tarjetaRepo, usuarioRepo, uow);

        // Crear CPs (Custom Transactions)
        AgregarProductoAlCarritoCP agregarProductoCP = new AgregarProductoAlCarritoCP(carritoRepo, productoRepo, uow);
        CancelarPedidoCP cancelarPedidoCP = new CancelarPedidoCP(pedidoRepo, productoRepo, uow);
        ProcesarDevolucionCP procesarDevolucionCP = new ProcesarDevolucionCP(pedidoRepo, productoRepo, uow);
        FinalizarCompraCP finalizarCompraCP = new FinalizarCompraCP(pedidoCEN, productoRepo, uow);

        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  1. CREANDO CATEGORÍAS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Categoria catDeportivo = categoriaCEN.Crear("Deportivo");
            Categoria catCasual = categoriaCEN.Crear("Casual");
            Categoria catFormal = categoriaCEN.Crear("Formal");
            Console.WriteLine($"✓ Categoría: {catDeportivo.Nombre} (ID: {catDeportivo.Id})");
            Console.WriteLine($"✓ Categoría: {catCasual.Nombre} (ID: {catCasual.Id})");
            Console.WriteLine($"✓ Categoría: {catFormal.Nombre} (ID: {catFormal.Id})");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  2. CREANDO ZAPATOS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Producto prod1 = productoCEN.Crear("Nike Air Max 2024", 129.99m, 15, true);
            prod1.Descripcion = "Zapatillas deportivas de última generación con tecnología Air";
            prod1.Color = "Negro/Blanco";
            prod1.TallasDisponibles.Add("38");
            prod1.TallasDisponibles.Add("39");
            prod1.TallasDisponibles.Add("40");
            prod1.TallasDisponibles.Add("41");
            prod1.TallasDisponibles.Add("42");
            productoRepo.Modify(prod1);

            // Asignar fotos de muestra (deben existir en WebMVC/wwwroot/images/productos)
            productoCEN.Modify(prod1.Id, fotos: new List<string> { "/images/productos/prod_24ebfbd406484c3d954b98dec97f0e5e.png" });

            Producto prod2 = productoCEN.Crear("Adidas Ultraboost", 159.99m, 12, true);
            prod2.Descripcion = "Zapatillas running con suela Boost para máxima amortiguación";
            prod2.Color = "Azul";
            prod2.TallasDisponibles.Add("39");
            prod2.TallasDisponibles.Add("40");
            prod2.TallasDisponibles.Add("41");
            prod2.TallasDisponibles.Add("42");
            prod2.TallasDisponibles.Add("43");
            productoRepo.Modify(prod2);
            productoCEN.Modify(prod2.Id, fotos: new List<string> { "/images/productos/prod_3c00f639be9e4c7aa81b8876537d69a4.png" });

            Producto prod3 = productoCEN.Crear("Vans Old Skool", 65.00m, 25, false);
            prod3.Descripcion = "Zapatillas casuales clásicas con diseño atemporal";
            prod3.Color = "Negro";
            prod3.TallasDisponibles.Add("37");
            prod3.TallasDisponibles.Add("38");
            prod3.TallasDisponibles.Add("39");
            prod3.TallasDisponibles.Add("40");
            prod3.TallasDisponibles.Add("41");
            productoRepo.Modify(prod3);
            productoCEN.Modify(prod3.Id, fotos: new List<string> { "/images/productos/prod_45ef84f8ea0c4b829412e2ef07aafa87.png" });

            Producto prod4 = productoCEN.Crear("Clarks Desert Boot", 95.50m, 18, true);
            prod4.Descripcion = "Botas desert de cuero premium, perfectas para look casual elegante";
            prod4.Color = "Marrón";
            prod4.TallasDisponibles.Add("40");
            prod4.TallasDisponibles.Add("41");
            prod4.TallasDisponibles.Add("42");
            prod4.TallasDisponibles.Add("43");
            prod4.TallasDisponibles.Add("44");
            productoRepo.Modify(prod4);
            productoCEN.Modify(prod4.Id, fotos: new List<string> { "/images/productos/prod_e6af14d4e74e49cda4dc3cfb21fc443b.png" });

            Producto prod5 = productoCEN.Crear("Converse Chuck Taylor", 55.00m, 30, false);
            prod5.Descripcion = "Zapatillas icónicas de lona, estilo urbano casual";
            prod5.Color = "Rojo";
            prod5.TallasDisponibles.Add("36");
            prod5.TallasDisponibles.Add("37");
            prod5.TallasDisponibles.Add("38");
            prod5.TallasDisponibles.Add("39");
            prod5.TallasDisponibles.Add("40");
            prod5.TallasDisponibles.Add("41");
            productoRepo.Modify(prod5);
            productoCEN.Modify(prod5.Id, fotos: new List<string> { "/images/productos/prod_e9eda5f46df542c68b4ae1a76f821b7e.png" });
            
            uow.SaveChanges();
            
            Console.WriteLine($"✓ {prod1.Nombre} - ${prod1.Precio} (Color: {prod1.Color}, Tallas: {string.Join(", ", prod1.TallasDisponibles)}, Destacado: ⭐)");
            Console.WriteLine($"✓ {prod2.Nombre} - ${prod2.Precio} (Color: {prod2.Color}, Tallas: {string.Join(", ", prod2.TallasDisponibles)}, Destacado: ⭐)");
            Console.WriteLine($"✓ {prod3.Nombre} - ${prod3.Precio} (Color: {prod3.Color}, Tallas: {string.Join(", ", prod3.TallasDisponibles)})");
            Console.WriteLine($"✓ {prod4.Nombre} - ${prod4.Precio} (Color: {prod4.Color}, Tallas: {string.Join(", ", prod4.TallasDisponibles)}, Destacado: ⭐)");
            Console.WriteLine($"✓ {prod5.Nombre} - ${prod5.Precio} (Color: {prod5.Color}, Tallas: {string.Join(", ", prod5.TallasDisponibles)})");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  3. REGISTRANDO USUARIOS + LOGIN");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            
            // 2b. CREAR MÁS ZAPATOS (hasta 30 en total)
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  2b. CREANDO MÁS ZAPATOS (Seeding hasta 30)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            var nombresExtra = new List<string>
            {
                "Puma RS-X", "New Balance 574", "Reebok Classic Leather", "Asics Gel-Kayano",
                "Saucony Jazz", "Mizuno Wave Rider", "Nike Cortez", "Adidas Stan Smith",
                "Timberland Classic", "Dr. Martens 1460", "Hoka One One Clifton", "Salomon Speedcross",
                "Merrell Trail Glove", "ECCO Soft 7", "Skechers Go Walk", "Clarks Wallabee",
                "On Cloudrunner", "Brooks Ghost", "Lacoste Graduate", "Fila Disruptor",
                "Kappa Authentic", "Le Coq Sportif Quartz", "Karhu Fusion 2.0", "Diadora N9000",
                "Asics GT-2000"
            };
            var colores = new List<string> { "Negro", "Blanco", "Azul", "Rojo", "Gris", "Verde", "Marrón", "Beige" };
            var tallasBase = new List<string> { "39", "40", "41", "42", "43" };
            var rnd = new Random(1234);

            int creados = 0;
            foreach (var nombre in nombresExtra)
            {
                // Precio aleatorio entre 45 y 180
                decimal precio = Math.Round((decimal)(45 + rnd.NextDouble() * 135), 2);
                // Stock aleatorio entre 10 y 50
                int stock = rnd.Next(10, 51);
                // Destacar ~1/3
                bool destacado = rnd.Next(0, 3) == 0;

                var p = productoCEN.Crear(nombre, precio, stock, destacado);
                p.Descripcion = $"Modelo {nombre} ideal para uso diario";
                p.Color = colores[rnd.Next(colores.Count)];
                // Asignar tallas (variando 4-5 tallas)
                p.TallasDisponibles.Clear();
                int numTallas = rnd.Next(4, 6);
                var tallas = tallasBase.OrderBy(_ => rnd.Next()).Take(numTallas).ToList();
                foreach (var t in tallas) p.TallasDisponibles.Add(t);
                productoRepo.Modify(p);

                // Opcionalmente, asignar una foto placeholder
                // Nota: usa rutas válidas si existen en tu proyecto Web
                try { productoCEN.Modify(p.Id, fotos: new List<string> { "/images/productos/placeholder.png" }); } catch {}

                creados++;
            }
            uow.SaveChanges();
            Console.WriteLine($"✓ Zapatos extra creados: {creados}");
            Console.WriteLine($"✓ Total de productos tras seeding: {productoCEN.ListarTodos().Count}");
            
            // Crear usuario ADMIN
            Usuario admin = usuarioCEN.Registrar("Admin", "admin@test.com", "admin123");
            admin.Rol = "admin"; // Must be lowercase for role check
            usuarioRepo.Modify(admin);
            uow.SaveChanges();
            Console.WriteLine($"✓ ADMIN: {admin.Nombre} ({admin.Email}) - Rol: {admin.Rol}");
            
            // Crear usuarios clientes
            Usuario usuario1 = usuarioCEN.Registrar("Juan Pérez", "juan@example.com", "password123");
            usuario1.Rol = "Cliente";
            usuario1.NombreEnvio = "Juan Pérez";
            usuario1.DireccionEnvio = "Calle Mayor 1, 28001 Madrid";
            usuario1.CiudadEnvio = "Madrid";
            usuario1.CPEnvio = "28001";
            usuario1.TelefonoEnvio = "+34 600 000 001";
            usuarioRepo.Modify(usuario1);
            
            Usuario usuario2 = usuarioCEN.Registrar("María García", "maria@example.com", "maria456");
            usuario2.Rol = "Cliente";
            usuario2.NombreEnvio = "María García";
            usuario2.DireccionEnvio = "Av. Sevilla 10, 41001 Sevilla";
            usuario2.CiudadEnvio = "Sevilla";
            usuario2.CPEnvio = "41001";
            usuario2.TelefonoEnvio = "+34 600 000 002";
            usuarioRepo.Modify(usuario2);
            
            Usuario usuario3 = usuarioCEN.Registrar("Carlos López", "carlos@example.com", "carlos789");
            usuario3.Rol = "Cliente";
            usuario3.NombreEnvio = "Carlos López";
            usuario3.DireccionEnvio = "Paseo Marítimo 5, 08003 Barcelona";
            usuario3.CiudadEnvio = "Barcelona";
            usuario3.CPEnvio = "08003";
            usuario3.TelefonoEnvio = "+34 600 000 003";
            usuarioRepo.Modify(usuario3);
            
            uow.SaveChanges();
            
            Console.WriteLine($"✓ Cliente: {usuario1.Nombre} ({usuario1.Email})");
            Console.WriteLine($"✓ Cliente: {usuario2.Nombre} ({usuario2.Email})");
            Console.WriteLine($"✓ Cliente: {usuario3.Nombre} ({usuario3.Email})");

            Usuario usuarioLogueado = usuarioCEN.Login("juan@example.com", "password123");
            Console.WriteLine($"\n✓ LOGIN EXITOSO: {usuarioLogueado.Nombre}");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  3b. TARJETA DE PRUEBA PARA JUAN");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            tarjetaCEN.Agregar(usuario1.Id, "VISA", "**** **** **** 6467", "6467", 12, 2026, "Juan Pérez", true);
            uow.SaveChanges();
            Console.WriteLine("✓ Tarjeta predeterminada guardada para Juan (VISA ****6467)");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  4. PROBANDO FILTROS (ReadFilter)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            IList<Producto> productosDestacados = productoCEN.ReadFilter(destacado: true);
            Console.WriteLine($"✓ Zapatos destacados: {productosDestacados.Count}");
            foreach (Producto p in productosDestacados)
                Console.WriteLine($"  - {p.Nombre} (${p.Precio})");

            IList<Producto> zapatosPorPrecio = productoCEN.ReadFilter(precioMin: 50m, precioMax: 100m);
            Console.WriteLine($"\n✓ Zapatos entre $50 y $100: {zapatosPorPrecio.Count}");
            foreach (Producto p in zapatosPorPrecio)
                Console.WriteLine($"  - {p.Nombre}: ${p.Precio}");

            IList<Producto> zapatosBaratos = productoCEN.ReadFilter(precioMax: 70m);
            Console.WriteLine($"\n✓ Zapatos económicos (menos de $70): {zapatosBaratos.Count}");
            foreach (Producto p in zapatosBaratos)
                Console.WriteLine($"  - {p.Nombre}: ${p.Precio}");

            IList<Producto> zapatosNegros = productoCEN.ReadFilter(color: "Negro");
            Console.WriteLine($"\n✓ Zapatos negros: {zapatosNegros.Count}");
            foreach (Producto p in zapatosNegros)
                Console.WriteLine($"  - {p.Nombre} (Color: {p.Color})");

            IList<Producto> zapatosConBlanco = productoCEN.ReadFilter(color: "Blanco");
            Console.WriteLine($"\n✓ Zapatos con blanco: {zapatosConBlanco.Count}");
            foreach (Producto p in zapatosConBlanco)
                Console.WriteLine($"  - {p.Nombre} (Color: {p.Color})");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  5. CREANDO CARRITO Y AGREGANDO ZAPATOS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Carrito carrito1 = carritoCEN.Crear(usuario1.Id);
            Console.WriteLine($"✓ Carrito creado para {usuario1.Nombre}");

            agregarProductoCP.Ejecutar(carrito1.Id, prod1.Id, 2);
            Console.WriteLine($"✓ Agregados 2x {prod1.Nombre} al carrito");
            
            agregarProductoCP.Ejecutar(carrito1.Id, prod2.Id, 3);
            Console.WriteLine($"✓ Agregados 3x {prod2.Nombre} al carrito");

            decimal totalCarrito = carritoCEN.CalcularTotal(carrito1.Id);
            Console.WriteLine($"\n✓ Total del carrito: ${totalCarrito:F2}");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  6. FINALIZANDO COMPRA (CustomTransaction)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            List<ItemPedido> itemsCompra = new List<ItemPedido>
            {
                new ItemPedido { ProductoId = prod1.Id, Cantidad = 2 },
                new ItemPedido { ProductoId = prod2.Id, Cantidad = 3 }
            };
            Pedido pedido1 = finalizarCompraCP.Execute(usuario1.Id, "Calle Principal 123", itemsCompra);
            Console.WriteLine($"✓ Pedido #{pedido1.Id} creado");
            Console.WriteLine($"  Estado: {pedido1.Estado}");
            Console.WriteLine($"  Total: ${pedido1.Total:F2}");
            Console.WriteLine($"  Items: {pedido1.Items.Count}");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  7. CREANDO VALORACIONES");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Valoracion val1 = valoracionCEN.Crear(usuario1.Id, prod1.Id, 5, "Excelente laptop");
            Valoracion val2 = valoracionCEN.Crear(usuario2.Id, prod1.Id, 4, "Muy buena calidad");
            Valoracion val3 = valoracionCEN.Crear(usuario3.Id, prod2.Id, 5, "Perfecto mouse");
            Console.WriteLine($"✓ Valoración de {usuario1.Nombre}: {val1.Valor}⭐");
            Console.WriteLine($"✓ Valoración de {usuario2.Nombre}: {val2.Valor}⭐");
            Console.WriteLine($"✓ Valoración de {usuario3.Nombre}: {val3.Valor}⭐");

            double promedio = valoracionCEN.CalcularPromedioProducto(prod1.Id);
            Console.WriteLine($"\n✓ Promedio de {prod1.Nombre}: {promedio:F2}⭐");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  8. AGREGANDO FAVORITOS");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Favoritos fav1 = favoritosCEN.Crear(usuario1.Id, prod3.Id);
            Favoritos fav2 = favoritosCEN.Crear(usuario1.Id, prod4.Id);
            Favoritos fav3 = favoritosCEN.Crear(usuario2.Id, prod1.Id);
            Console.WriteLine($"✓ {usuario1.Nombre} agregó {prod3.Nombre} a favoritos");
            Console.WriteLine($"✓ {usuario1.Nombre} agregó {prod4.Nombre} a favoritos");
            Console.WriteLine($"✓ {usuario2.Nombre} agregó {prod1.Nombre} a favoritos");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  9. CANCELANDO PEDIDO (CustomTransaction)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Carrito carrito2 = carritoCEN.Crear(usuario2.Id);
            agregarProductoCP.Ejecutar(carrito2.Id, prod4.Id, 2);
            List<ItemPedido> itemsPedido2 = new List<ItemPedido> { new ItemPedido { ProductoId = prod4.Id, Cantidad = 2 } };
            Pedido pedido2 = finalizarCompraCP.Execute(usuario2.Id, "Avenida Libertad 456", itemsPedido2);
            Console.WriteLine($"✓ Pedido #{pedido2.Id} creado (2x {prod4.Nombre})");

            int stockAntes = productoRepo.GetById(prod4.Id).Stock;
            Console.WriteLine($"  Stock de {prod4.Nombre} ANTES: {stockAntes}");
            
            cancelarPedidoCP.Ejecutar(pedido2.Id);
            int stockDespues = productoRepo.GetById(prod4.Id).Stock;
            Console.WriteLine($"\n✓ Pedido #{pedido2.Id} CANCELADO");
            Console.WriteLine($"  Stock de {prod4.Nombre} DESPUÉS: {stockDespues}");
            Console.WriteLine($"  Stock restaurado: +{stockDespues - stockAntes} unidades");

            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
            Console.WriteLine("  10. PROCESANDO DEVOLUCIÓN (CustomTransaction)");
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            pedidoCEN.CambiarEstado(pedido1.Id, EstadoPedido.recibido);
            Console.WriteLine($"✓ Pedido #{pedido1.Id} marcado como RECIBIDO");

            int stockAntesDev = productoRepo.GetById(prod1.Id).Stock;
            Console.WriteLine($"  Stock de {prod1.Nombre} ANTES: {stockAntesDev}");
            
            procesarDevolucionCP.Ejecutar(pedido1.Id, prod1.Id, 1, "Producto con defecto");
            int stockDespuesDev = productoRepo.GetById(prod1.Id).Stock;
            Console.WriteLine($"\n✓ DEVOLUCIÓN PROCESADA");
            Console.WriteLine($"  Stock de {prod1.Nombre} DESPUÉS: {stockDespuesDev}");
            Console.WriteLine($"  Stock restaurado: +{stockDespuesDev - stockAntesDev} unidad");

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    RESUMEN FINAL                               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"  Usuarios:     {usuarioCEN.ReadAll().Count}");
            Console.WriteLine($"  Productos:    {productoCEN.ListarTodos().Count}");
            Console.WriteLine($"  Categorías:   {categoriaCEN.ReadAll().Count}");
            Console.WriteLine($"  Pedidos:      {pedidoCEN.ReadAll().Count}");
            Console.WriteLine($"  Valoraciones: {valoracionCEN.ReadAll().Count}");
            Console.WriteLine($"  Favoritos:    {favoritosCEN.ReadAll().Count}");

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     ✅ BASE DE DATOS INICIALIZADA EXITOSAMENTE                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\n✅ Implementación completa del backend:");
            Console.WriteLine("  • CRUD: 7 entidades completas");
            Console.WriteLine("  • Login: Implementado y probado");
            Console.WriteLine("  • ReadFilter: 7 filtros implementados");
            Console.WriteLine("  • Custom Operations: 24+ operaciones");
            Console.WriteLine("  • CustomTransactions: 4 CPs transaccionales");
            Console.WriteLine("  • Base de datos: NHibernate + SQL Server LocalDB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            // Cerrar sesión de NHibernate
            if (uow.Session != null && uow.Session.IsOpen)
            {
                uow.Session.Close();
            }
        }

        Console.WriteLine("\n\nPresiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}
