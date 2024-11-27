using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Repositories;
using trackit.server.Services;
using trackit.server.Patterns.Observer; // Importar el patrón Observer
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using trackit.server.Middlewares;
using trackit.server.Services.Interfaces;
using trackit.server.Factories.UserFactories;
using CloudinaryDotNet;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS: permite solicitudes desde http://localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // Permitir solicitudes desde localhost:3000
                  .AllowAnyHeader()    // Permitir cualquier encabezado
                  .AllowAnyMethod();   // Permitir cualquier método (GET, POST, PUT, DELETE, etc.)
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer {token}' en el campo de autorización."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// **Inyección de dependencias del Patrón Observer**
builder.Services.AddSingleton<RequirementNotifier>(); // Registrar el notificador como Singleton
builder.Services.AddScoped<IObserver, InternalNotificationObserver>(); // Observador de notificaciones internas
builder.Services.AddScoped<IObserver, EmailNotificationObserver>();   // Observador de notificaciones por correo
builder.Services.AddScoped<IObserver, ActionLogObserver>();
// Inyección de dependencias generales
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Registrar las factorías específicas
builder.Services.AddScoped<IInternalUserFactory, InternalUserFactory>();
builder.Services.AddScoped<IExternalUserFactory, ExternalUserFactory>();

// Configuración de controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Registrar repositorios y servicios para Requirements
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IRequirementService, RequirementService>(); // Usa el RequirementNotifier
builder.Services.AddScoped<IRequirementActionLogRepository, RequirementActionLogRepository>();
builder.Services.AddScoped<IRequirementActionService, RequirementActionService>();
builder.Services.AddScoped<IRequirementTypeRepository, RequirementTypeRepository>();
builder.Services.AddScoped<IRequirementTypeService, RequirementTypeService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Registrar middleware de manejo de excepciones
builder.Services.AddTransient<AppInitializationService>(); // Servicio de inicialización

var app = builder.Build();
var notifier = app.Services.GetRequiredService<RequirementNotifier>();

// Configurar middleware para manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Middleware de manejo de excepciones debe estar primero

// Ejecutar la creación del Admin si no existe al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var emailObserver = new EmailNotificationObserver(emailService);

    // Adjuntar el observador de correo
    notifier.Attach(emailObserver);

    // Puedes agregar otros observadores si es necesario
}
// Middleware de manejo de excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Inicializar Admin y roles
using (var scope = app.Services.CreateScope())
{
    var appInitializationService = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await appInitializationService.CreateAdminIfNotExistsAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoles(roleManager);
}

// Asegúrate de usar CORS antes de cualquier otro middleware (como autenticación y autorización)
app.UseCors("AllowLocalhost"); // Aplica la política CORS

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

// Middleware adicional para autenticación y autorización
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de controladores
app.MapControllers();
app.Run();
