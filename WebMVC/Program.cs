using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configurar cadena de conexión: fuerza SQLEXPRESS si PREFER_SQLEXPRESS=1; si no, usa NH_CONNECTION si existe; en último caso LocalDB
var preferExpress = string.Equals(Environment.GetEnvironmentVariable("PREFER_SQLEXPRESS"), "1", StringComparison.OrdinalIgnoreCase);
var envConnExisting = Environment.GetEnvironmentVariable("NH_CONNECTION");
var connExpress = "Server=localhost\\SQLEXPRESS;Database=TiendaZapatos;Trusted_Connection=True;TrustServerCertificate=True;";
var connLocalDb = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TiendaZapatos;Integrated Security=True;";

string finalConn = connLocalDb;
if (preferExpress)
{
    finalConn = connExpress; // fuerza SQLEXPRESS
}
else if (!string.IsNullOrWhiteSpace(envConnExisting))
{
    finalConn = envConnExisting;
}
Environment.SetEnvironmentVariable("NH_CONNECTION", finalConn);

// SessionFactory singleton
builder.Services.AddSingleton<ISessionFactory>(_ =>
{
    var cfg = NHibernateHelper.BuildConfiguration();
    // Usa siempre la connection de NH_CONNECTION que fijamos arriba
    var envConn = Environment.GetEnvironmentVariable("NH_CONNECTION");
    cfg.SetProperty("connection.connection_string", string.IsNullOrWhiteSpace(envConn) ? finalConn : envConn);
    cfg.SetProperty("show_sql", "true");
    cfg.SetProperty("format_sql", "true");
    try
    {
        var activeConn = cfg.GetProperty("connection.connection_string") ?? "(none)";
        Console.WriteLine($"NHibernate connection string: {activeConn}");
    }
    catch { }
    return cfg.BuildSessionFactory();
});

// ISession scoped
builder.Services.AddScoped<NHibernate.ISession>(sp =>
{
    var factory = sp.GetRequiredService<ISessionFactory>();
    return factory.OpenSession();
});

// UoW scoped
builder.Services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();
builder.Services.AddScoped<NHibernateUnitOfWork>();

// Repositorios
builder.Services.AddScoped<IProductoRepository, NHibernateProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, NHibernatePedidoRepository>();
builder.Services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
builder.Services.AddScoped<ICarritoRepository, NHibernateCarritoRepository>();
builder.Services.AddScoped<ICategoriaRepository, NHibernateCategoriaRepository>();
builder.Services.AddScoped<IValoracionRepository, NHibernateValoracionRepository>();
builder.Services.AddScoped<IFavoritosRepository, NHibernateFavoritosRepository>();
builder.Services.AddScoped<ITarjetaRepository, NHibernateTarjetaRepository>();
builder.Services.AddScoped<IRepository<ApplicationCore.Domain.EN.Carrito,long>>(sp => sp.GetRequiredService<ICarritoRepository>());

// CENs y CPs
builder.Services.AddScoped<ProductoCEN>();
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<PedidoCEN>();
builder.Services.AddScoped<CarritoCEN>();
builder.Services.AddScoped<CategoriaCEN>();
builder.Services.AddScoped<ValoracionCEN>();
builder.Services.AddScoped<FavoritosCEN>();
builder.Services.AddScoped<TarjetaCEN>();
builder.Services.AddScoped<PedidoCEN>();
builder.Services.AddScoped<AgregarProductoAlCarritoCP>();
builder.Services.AddScoped<FinalizarCompraCP>();
builder.Services.AddScoped<CancelarPedidoCP>();
builder.Services.AddScoped<ProcesarDevolucionCP>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Rewrite legacy placeholder.png to existing placeholder.svg
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (!string.IsNullOrEmpty(path) && string.Equals(path, "/images/productos/placeholder.png", StringComparison.OrdinalIgnoreCase))
    {
        context.Request.Path = "/images/productos/placeholder.svg";
    }
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tienda}/{action=IndexPublico}/{id?}");

app.Run();
