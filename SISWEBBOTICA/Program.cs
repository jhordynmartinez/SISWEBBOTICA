using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

//--------------------------------------------------------------------
// 1. CONFIGURACIÓN DE SERVICIOS
//--------------------------------------------------------------------

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("CadenaSQL");
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IUnidadMedidaRepository, UnidadMedidaRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IMLService, MLService>();

builder.Services.AddIdentity<Usuario, TipoUsuario>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<AppDBContext>()
.AddDefaultTokenProviders();

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
app.UseStaticFiles(); // 1. Servir archivos estáticos (CSS, JS)
app.UseRouting();     // 2. Determinar qué endpoint se va a usar

// 3. Es crucial que la autenticación vaya antes de la autorización
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        // Las migraciones se aplican manualmente
        // await context.Database.MigrateAsync();

        try
        {
            // Seed de PÚBLICO GENERAL - Usar DateTime.Now para consistencia
            var publicoGeneral = await context.Clientes
                .FirstOrDefaultAsync(c => c.Nombre == "PÚBLICO GENERAL");
            
            if (publicoGeneral == null)
            {
                publicoGeneral = new Cliente
                {
                    Nombre = "PÚBLICO GENERAL",
                    RucDni = "00000000",
                    Estado = "Activo",
                    FechaRegistro = DateTime.Now,
                    EsClienteVIP = false
                };
                context.Clientes.Add(publicoGeneral);
                await context.SaveChangesAsync();
                logger.LogInformation("Cliente 'PÚBLICO GENERAL' creado exitosamente.");
            }
            else
            {
                logger.LogInformation("Cliente 'PÚBLICO GENERAL' ya existe.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar cliente 'PÚBLICO GENERAL'.");
        }

        try
        {
            if (!context.MetodosPago.Any())
            {
                context.MetodosPago.AddRange(
                    new MetodoPago { Nombre = "Efectivo", RequiereReferencia = false },
                    new MetodoPago { Nombre = "Yape", RequiereReferencia = true },
                    new MetodoPago { Nombre = "Plin", RequiereReferencia = true },
                    new MetodoPago { Nombre = "Tarjeta", RequiereReferencia = true }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Métodos de pago creados exitosamente.");
            }
            else
            {
                logger.LogInformation("Métodos de pago ya existen.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar métodos de pago.");
        }

        try
        {
            if (!context.Monedas.Any())
            {
                context.Monedas.Add(new Moneda { Nombre = "NUEVOS SOLES", Simbolo = "S/." });
                await context.SaveChangesAsync();
                logger.LogInformation("Moneda 'NUEVOS SOLES' creada exitosamente.");
            }
            else
            {
                logger.LogInformation("Moneda ya existe.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar moneda.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error general al inicializar la base de datos.");
    }
}

//--------------------------------------------------------------------
// 5. EJECUTAR LA APLICACIÓN
//--------------------------------------------------------------------
app.Run();