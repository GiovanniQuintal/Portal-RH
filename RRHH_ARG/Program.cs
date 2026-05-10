using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using RRHH_ARG.Components;
using RRHH_ARG.Components.Account;
using RRHH_ARG.Data;
using RRHH_ARG.Services;
using System.Net.Http;


var builder = WebApplication.CreateBuilder(args);


// ==========================================
// 1. CONFIGURACI�N DE BASE DE DATOS
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
  //  options.UseSqlServer(connectionString));

// --- AGREGA ESTAS DOS L�NEAS AQU� ---
builder.Services.AddControllers(); // Ya la ten�as (para tu ArchivoController)
builder.Services.AddRazorPages();  // <--- �ESTA ES LA QUE FALTA!
// ------------------------------------

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ==========================================
// 2. TUS SERVICIOS DE APLICACI�N (SCOPED)
// ==========================================
// Registrar el servicio como Scoped (Correcto)
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<PasivoLaboralService>();

// ==========================================
// 3. CONFIGURACI�N DE IDENTITY Y AUTH
// ==========================================
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// ==========================================
// 4. CONFIGURACI�N DE COMPONENTES UI (Blazor + FluentUI)
// ==========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();

// ==========================================
// 5. CONSTRUCCI�N DE LA APP (SOLO UNA VEZ)
// ==========================================

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        // Límite de mensaje (Bytes). 100MB es bastante seguro.
        options.MaximumReceiveMessageSize = 1024 * 1024 * 100;

        // Aumentar Timeout para conexiones lentas
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
        options.HandshakeTimeout = TimeSpan.FromMinutes(1);
    });

// ==========================================
// 6. CONFIGURACION DE SERVICIO MOOODLE SYNC
// ==========================================

// (Lógica para ignorar SSL):
builder.Services.AddHttpClient<RRHH_ARG.Services.MoodleSyncService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Esta línea es la clave: devuelve 'true' siempre, ignorando errores de SSL
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

builder.Services.AddScoped<PayrollService>();

builder.Services.AddHostedService<RRHH_ARG.Services.MoodleBackgroundService>();
builder.Services.AddBlazorBootstrap();

builder.Services.AddScoped<PdService>();
builder.Services.AddScoped<VacacionService>();



// Mantenemos AddDbContext para Identity, pero agregamos la Factory para nuestros Servicios
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // O tu configuración actual

var app = builder.Build();

// Configuraci�n del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// =========================================================
// 1. FORZAR LA CULTURA GLOBAL (MÉXICO) PARA NÚMEROS Y FECHAS
// =========================================================
var supportedCultures = new[] { "es-MX" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
// =========================================================

app.UseHttpsRedirection();
app.UseStaticFiles();

// ====================================================================
// CORRECCI�N DEL ORDEN (PIPELINE)
// El error ocurr�a porque Antiforgery estaba ANTES de Routing
// ====================================================================

app.UseRouting();          // 1. Primero definimos la ruta
app.UseAuthentication();   // 2. Luego identificamos al usuario
app.UseAuthorization();    // 3. Luego verificamos permisos
app.UseAntiforgery();      // 4. FINALMENTE validamos el token de seguridad

// ====================================================================

// --- AQU� EST�N LAS RUTAS ---
app.MapControllers(); // <--- IMPORTANTE: Hace que /api/Archivo/... funcione

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Esto es necesario para Identity
app.MapAdditionalIdentityEndpoints();

app.Run();