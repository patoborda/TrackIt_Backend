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

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSignalR(); // 🔹 Agregar SignalR

// Configuración de Logging
builder.Services.AddLogging(configure => configure.AddConsole());


// Configuración de Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured.");
}


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
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

// Configurar Swagger
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Inyección de dependencias del Patrón Observer
builder.Services.AddSingleton<RequirementNotifier>();
builder.Services.AddScoped<IObserver, InternalNotificationObserver>();
builder.Services.AddScoped<IObserver, EmailNotificationObserver>();
builder.Services.AddScoped<IObserver, ActionLogObserver>();

// 🔹 Inyección de dependencias de Servicios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// 🔹 Registrar las factorías de usuarios
builder.Services.AddScoped<IInternalUserFactory, InternalUserFactory>();
builder.Services.AddScoped<IExternalUserFactory, ExternalUserFactory>();

// 🔹 Repositorios y Servicios de Requirements
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IRequirementActionLogRepository, RequirementActionLogRepository>();
builder.Services.AddScoped<IRequirementActionService, RequirementActionService>();
builder.Services.AddScoped<IRequirementTypeRepository, RequirementTypeRepository>();
builder.Services.AddScoped<IRequirementTypeService, RequirementTypeService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Configuration.AddJsonFile("appsettings.sensitive.json", optional: true, reloadOnChange: true);

// Middleware de Manejo de Excepciones
builder.Services.AddTransient<AppInitializationService>();

var app = builder.Build();

// 🔹 Configurar Middleware de Excepciones (Una sola vez)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 🔹 Configurar CORS (Antes de Autenticación)
app.UseCors("AllowLocalhost");

// 🔹 Configurar Middleware de Seguridad
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.MapHub<CommentHub>("/commentHub");
app.MapControllers();


// 🔹 Configurar Observadores en `RequirementNotifier`
using (var scope = app.Services.CreateScope())
{
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var emailObserver = new EmailNotificationObserver(emailService);
    var notifier = scope.ServiceProvider.GetRequiredService<RequirementNotifier>();

    notifier.Attach(emailObserver);
}

// 🔹 Ejecutar creación del Admin y roles después de iniciar la app
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var appInitializationService = scope.ServiceProvider.GetRequiredService<AppInitializationService>();
    await appInitializationService.CreateAdminIfNotExistsAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedRoles(roleManager);
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = string.Empty; // Hace que Swagger esté disponible en la raíz
    });
}

// 🔹 Iniciar la aplicación
app.MapControllers();
app.Run();
