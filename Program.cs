using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Patterns.Observer;
using trackit.server.Middlewares;
using trackit.server.Hubs;
using trackit.server.Extensions;
using CloudinaryDotNet;
using Microsoft.OpenApi.Models;
using trackit.server.Services;
using trackit.server.Repositories;

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
builder.Services.ConfigureSignalR();

var app = builder.Build();

// 🔹 Configurar middleware (orden correcto)
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Primero para manejar errores globales
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Logs estructurados para autenticación
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

// 🔹 Configurar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Inicialización de Admin y Roles al iniciar
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var initService = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await initService.CreateAdminIfNotExistsAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoles(roleManager);
});

// 🔹 Configurar Hubs y Controladores
app.MapHub<CommentHub>("/commentHub");
app.MapControllers();
app.Run();
