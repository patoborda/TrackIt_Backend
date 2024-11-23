using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using trackit.server.Models;

public class AppInitializationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration; // Inyectar IConfiguration

    public AppInitializationService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration; // Guardar la configuración
    }

    public async Task CreateAdminIfNotExistsAsync()
    {
        // Leer las configuraciones desde appsettings.sensitive.json
        var adminEmail = _configuration["AdminSettings:Email"]; // Asegúrate de que la clave esté correctamente configurada
        var adminPassword = _configuration["AdminSettings:Password"]; // Y que la clave coincida

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin email or password is not configured.");
        }

        var user = await _userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            // Crear un nuevo usuario admin si no existe
            var adminUser = new AdminUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                IsEnabled = true,  // Asegúrate de que el admin esté habilitado
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Verificar si el rol Admin existe, si no lo crea
                var roleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
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
            else
            {
                throw new Exception("Error creating admin user.");
            }
        }
    }

}
