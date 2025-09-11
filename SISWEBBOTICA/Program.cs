using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// 1. Obtener la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");

// 2. Registrar el AppDBContext con el proveedor de SQL Server
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

// --- FIN DEL CÓDIGO A AGREGAR ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
