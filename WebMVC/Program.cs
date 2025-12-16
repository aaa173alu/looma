using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using NHibernate;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Usar SQL Server Express 2019
var connectionString = "Server=localhost\\SQLEXPRESS;Database=TiendaZapatos;Trusted_Connection=True;TrustServerCertificate=True;";
Environment.SetEnvironmentVariable("NH_CONNECTION", connectionString);

// SessionFactory singleton
builder.Services.AddSingleton<ISessionFactory>(_ =>
{
    var cfg = NHibernateHelper.BuildConfiguration();
    var envConn = Environment.GetEnvironmentVariable("NH_CONNECTION");
    if (!string.IsNullOrWhiteSpace(envConn))
        cfg.SetProperty("connection.connection_string", envConn);
    cfg.SetProperty("show_sql", "true");
    cfg.SetProperty("format_sql", "true");
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
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
