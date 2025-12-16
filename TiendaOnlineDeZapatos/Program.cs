using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Configurar NHibernate como Singleton
builder.Services.AddScoped<NHibernateUnitOfWork>();

// Registrar repositorios
builder.Services.AddScoped<IProductoRepository, NHibernateProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, NHibernatePedidoRepository>();
builder.Services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
builder.Services.AddScoped<ICarritoRepository, NHibernateCarritoRepository>();
builder.Services.AddScoped<ICategoriaRepository, NHibernateCategoriaRepository>();
builder.Services.AddScoped<IValoracionRepository, NHibernateValoracionRepository>();
builder.Services.AddScoped<IFavoritosRepository, NHibernateFavoritosRepository>();

// Registrar CENs
builder.Services.AddScoped<ProductoCEN>();
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<PedidoCEN>();
builder.Services.AddScoped<CarritoCEN>();
builder.Services.AddScoped<CategoriaCEN>();
builder.Services.AddScoped<ValoracionCEN>();
builder.Services.AddScoped<FavoritosCEN>();

// Registrar CPs
builder.Services.AddScoped<AgregarProductoAlCarritoCP>();
builder.Services.AddScoped<FinalizarCompraCP>();
builder.Services.AddScoped<CancelarPedidoCP>();
builder.Services.AddScoped<ProcesarDevolucionCP>();

// Configurar sesión para mantener login
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();