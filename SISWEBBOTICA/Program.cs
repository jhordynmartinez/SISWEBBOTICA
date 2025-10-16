using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Services; // <-- AÑADIR ESTE USING
using System;

var builder = WebApplication.CreateBuilder(args);

//--------------------------------------------------------------------
// 1. CONFIGURACIÓN DE SERVICIOS
//--------------------------------------------------------------------

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

// --- INICIO DE LA MODIFICACIÓN ---
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IUnidadMedidaRepository, UnidadMedidaRepository>();

// --- FIN DE LA MODIFICACIÓN ---

// --- INICIO DE LA CORRECCIÓN: Configuración de ASP.NET Core Identity ---
builder.Services.AddIdentity<Usuario, TipoUsuario>(options => {
    // Configuración de las reglas de contraseña para cumplir con la HU
    options.Password.RequireDigit = true;         // Requiere al menos un número
    options.Password.RequireLowercase = true;     // Requiere al menos una minúscula
    options.Password.RequireNonAlphanumeric = false; // No requiere símbolos especiales
    options.Password.RequireUppercase = false;    // No requiere mayúsculas
    options.Password.RequiredLength = 8;          // Mínimo 8 caracteres
    options.Password.RequiredUniqueChars = 1;     // Caracteres únicos requeridos
})
.AddEntityFrameworkStores<AppDBContext>()
.AddDefaultTokenProviders();
// --- FIN DE LA CORRECCIÓN ---

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//--------------------------------------------------------------------
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
//--------------------------------------------------------------------
var app = builder.Build();

//--------------------------------------------------------------------
// 3. CONFIGURACIÓN DEL PIPELINE DE SOLICITUDES HTTP
//--------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

//--------------------------------------------------------------------
// 4. INICIALIZACIÓN DE DATOS (DATA SEEDING)
//--------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        await context.Database.MigrateAsync();

        // Crear Cliente "PÚBLICO GENERAL" si no existe
        if (!context.Clientes.Any(c => c.Nombre == "PÚBLICO GENERAL"))
        {
            context.Clientes.Add(new Cliente { Nombre = "PÚBLICO GENERAL", RucDni = "00000000" });
            await context.SaveChangesAsync();
        }

        // Crear Métodos de Pago si no existen
        if (!context.MetodosPago.Any())
        {
            context.MetodosPago.AddRange(
                new MetodoPago { Nombre = "Efectivo", RequiereReferencia = false },
                new MetodoPago { Nombre = "Yape", RequiereReferencia = true },
                new MetodoPago { Nombre = "Plin", RequiereReferencia = true },
                new MetodoPago { Nombre = "Tarjeta", RequiereReferencia = true }
            );
            await context.SaveChangesAsync();
        }

        // Crear Moneda por defecto si no existe
        if (!context.Monedas.Any())
        {
            context.Monedas.Add(new Moneda { Nombre = "NUEVOS SOLES", Simbolo = "S/." });
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

//--------------------------------------------------------------------
// 5. EJECUTAR LA APLICACIÓN
//--------------------------------------------------------------------
app.Run();