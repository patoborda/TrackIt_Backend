using Microsoft.AspNetCore.Identity;
using trackit.server.Extensions;
using trackit.server.Middlewares;
using trackit.server.Services;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.sensitive.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔹 Configurar servicios (métodos de extensión)
builder.Services.ConfigureCors();
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureCloudinary(builder.Configuration);
builder.Services.ConfigureObserverPattern();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();
builder.Services.AddSignalR();

var app = builder.Build();

// 🔹 Configurar middleware (orden correcto)
// 1. Middleware para manejo de excepciones globales
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Habilitar el enrutamiento
app.UseRouting();
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Primero para manejar errores globales
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();

// 5. Middleware de logs para el header de autorización
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        logger.LogInformation("Authorization Header: {AuthHeader}", authHeader);
    }
    else
    {
        logger.LogWarning("No Authorization Header Found");
    }
    await next.Invoke();
});

// 6. Configurar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7. Inicialización de Admin y Roles al iniciar
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var initService = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await initService.CreateAdminIfNotExistsAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoles(roleManager);
});

// 8. Configurar Hubs y Controladores
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.MapControllers();

app.Run();
