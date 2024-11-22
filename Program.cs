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
using trackit.server.Middlewares;  // Importa la carpeta donde tienes el middleware
using trackit.server.Services.Interfaces;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddLogging(configure => configure.AddConsole());

// Configurar la conexi�n con la base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Obtener la clave JWT de la configuraci�n
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
}

// Configurar JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;  // Puedes ponerlo en true en producci�n
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

 
// Inyecci�n de dependencias
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();  // Registro del servicio de correo
builder.Services.AddScoped<IAuthService, AuthService>();

// Configurar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories and services
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IRequirementActionLogRepository, RequirementActionLogRepository>();
builder.Services.AddScoped<IRequirementActionService, RequirementActionService>();
builder.Services.AddSingleton<RequirementNotifier>();
builder.Services.AddTransient<IRequirementObserver, ActionLogObserver>();

var app = builder.Build();

// Attach the observer to the notifier
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var notifier = services.GetRequiredService<RequirementNotifier>();
    var observer = services.GetRequiredService<IRequirementObserver>();
    notifier.Attach(observer); // Attach observer to notifier
}
// Configurar middleware para manejo de excepciones - Primer middleware en la cadena
app.UseMiddleware<ExceptionHandlingMiddleware>();  // Aseg�rate de que este middleware est� registrado primero


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Middleware de autenticaci�n y autorizaci�n
app.UseAuthentication();  // Primero autenticamos
app.UseAuthorization();   // Luego autorizamos

app.UseHttpsRedirection();
// Mapeo de controladores
app.MapControllers();
app.Run();
