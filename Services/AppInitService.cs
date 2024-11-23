using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using trackit.server.Models;
using trackit.server.Data;

public class AppInitializationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserDbContext _context; // Inyectar ApplicationDbContext
    private readonly IConfiguration _configuration;

    public AppInitializationService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task CreateAdminIfNotExistsAsync()
    {
        // Leer las configuraciones desde appsettings.sensitive.json
        var adminEmail = _configuration["AdminSettings:Email"];
        var adminPassword = _configuration["AdminSettings:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin email or password is not configured.");
        }

        // Verificar si el usuario Admin ya existe
        var user = await _userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            // Crear el usuario base en AspNetUsers
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                IsEnabled = true // El admin siempre está habilitado
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                throw new Exception($"Error creating admin user: {string.Join(", ", result.Errors)}");
            }

            // Crear la entrada correspondiente en la tabla AdminUsers
            var adminDetails = new AdminUser
            {
                Id = adminUser.Id // Relación con el usuario base
            };

            _context.AdminUsers.Add(adminDetails);
            await _context.SaveChangesAsync();

            // Verificar si el rol "Admin" existe, si no lo crea
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                {
                    throw new Exception("Error creating Admin role.");
                }
            }

            // Asignar el rol Admin al usuario
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
