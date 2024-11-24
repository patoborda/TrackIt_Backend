using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Repositories;
using trackit.server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using trackit.server.Middlewares;
using trackit.server.Services.Interfaces;
using trackit.server.Factories.UserFactories;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Configurar la conexión con la base de datos
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

// Cargar appsettings.json y appsettings.sensitive.json
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.sensitive.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ahora obtén la clave JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured in appsettings.json or environment.");
}

// Configurar JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Configuración de Cloudinary
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
var cloudinaryAccount = new Account(
    cloudinaryConfig["CloudName"],
    cloudinaryConfig["ApiKey"],
    cloudinaryConfig["ApiSecret"]
);

// Registramos Cloudinary como Singleton
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

// Configuración de Swagger con autenticación JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese 'Bearer {token}' en el campo de autorización."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Inyección de dependencias
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();  // Servicio de correo
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Registrar las factorías específicas
builder.Services.AddScoped<IInternalUserFactory, InternalUserFactory>();
builder.Services.AddScoped<IExternalUserFactory, ExternalUserFactory>();

// Configurar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Registrar repositorios y servicios para Requirements
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IRequirementActionLogRepository, RequirementActionLogRepository>();
builder.Services.AddScoped<IRequirementActionService, RequirementActionService>();
builder.Services.AddSingleton<RequirementNotifier>();
builder.Services.AddTransient<IRequirementObserver, ActionLogObserver>();
builder.Services.AddTransient<AppInitializationService>(); // Servicio de inicialización
builder.Services.AddScoped<IRequirementTypeRepository, RequirementTypeRepository>();
builder.Services.AddScoped<IRequirementTypeService, RequirementTypeService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

var app = builder.Build();

// Configurar middleware para manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Middleware de manejo de excepciones debe estar primero

// Ejecutar la creación del Admin si no existe al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var appInitializationService = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await appInitializationService.CreateAdminIfNotExistsAsync(); // Crear admin
}

// Configurar roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoles(roleManager); // Sembrar roles si es necesario
}
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        Console.WriteLine("Authorization Header Found: " + context.Request.Headers["Authorization"]);
    }
    else
    {
        Console.WriteLine("No Authorization Header Found");
    }
    await next.Invoke();
});

// Configurar la tubería de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de autenticación y autorización
app.UseAuthentication(); // Primero autenticamos
app.UseAuthorization();  // Luego autorizamos

// Redirigir a HTTPS si es necesario
app.UseHttpsRedirection();

// Mapeo de controladores
app.MapControllers();
app.Run();
