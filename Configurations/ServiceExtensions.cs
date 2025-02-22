using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Patterns.Observer;
using trackit.server.Repositories;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services;
using trackit.server.Services.Interfaces;
using trackit.server.Factories.UserFactories;
using CloudinaryDotNet;

namespace trackit.server.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }

        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration config)
        {
            var jwtKey = config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
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
        }

        public static void ConfigureCloudinary(this IServiceCollection services, IConfiguration config)
        {
            var cloudinaryConfig = config.GetSection("Cloudinary");
            var cloudinaryAccount = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );

            services.AddSingleton(new Cloudinary(cloudinaryAccount));
        }

        public static void ConfigureObserverPattern(this IServiceCollection services)
        {
            services.AddSingleton<RequirementNotifier>();
            services.AddScoped<IObserver, InternalNotificationObserver>();
            services.AddScoped<IObserver, EmailNotificationObserver>();
            services.AddScoped<IObserver, ActionLogObserver>();
        }

        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRequirementRepository, RequirementRepository>();
            services.AddScoped<IRequirementActionLogRepository, RequirementActionLogRepository>();
            services.AddScoped<IRequirementTypeRepository, RequirementTypeRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IPriorityRepository, PriorityRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IRequirementService, RequirementService>();
            services.AddScoped<IRequirementActionService, RequirementActionService>();
            services.AddScoped<IRequirementTypeService, RequirementTypeService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPriorityService, PriorityService>();
            services.AddScoped<IInternalUserFactory, InternalUserFactory>();
            services.AddScoped<IExternalUserFactory, ExternalUserFactory>();
            services.AddTransient<AppInitializationService>();
        }

        public static void ConfigureSignalR(this IServiceCollection services)
        {
            services.AddSignalR();
        }
    }
}
