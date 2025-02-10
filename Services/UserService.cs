using Microsoft.AspNetCore.Identity;
using trackit.server.Dtos;
using trackit.server.Repositories.Interfaces;
using trackit.server.Models;
using trackit.server.Exceptions;
using trackit.server.Services.Interfaces;
using trackit.server.Factories.UserFactories;
using static trackit.server.Dtos.RegisterUserDto;
using Microsoft.EntityFrameworkCore.Metadata;

namespace trackit.server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IInternalUserFactory _internalUserFactory;
        private readonly IExternalUserFactory _externalUserFactory;
        private readonly IImageService _imageService;

        // Constructor actualizado con las fábricas específicas
        public UserService(IUserRepository userRepository,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            UserManager<User> userManager,
            IEmailService emailService,
            IInternalUserFactory internalUserFactory,
            IExternalUserFactory externalUserFactory,
            IImageService imageService)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _configuration = configuration;
            _userManager = userManager;
            _emailService = emailService;
            _internalUserFactory = internalUserFactory;
            _externalUserFactory = externalUserFactory;
            _imageService = imageService;
        }

        public async Task<bool> RegisterInternalUserAsync(RegisterInternalUserDto registerInternalUserDto)
        {
            try
            {
                // Verificación de contraseñas
                if (registerInternalUserDto.Password != registerInternalUserDto.ConfirmPassword)
                    throw new PasswordMismatchException();

                // Usar la fábrica para crear el usuario interno
                var internalUser = _internalUserFactory.CreateUser(
                    registerInternalUserDto.Email,
                    registerInternalUserDto.FirstName,
                    registerInternalUserDto.LastName,
                    registerInternalUserDto.Password,
                    registerInternalUserDto.Cargo,
                    registerInternalUserDto.Departamento
                );

                // Verificar si el rol "Interno" existe
                var roleExists = await _userRepository.RoleExistsAsync("Interno");
                if (!roleExists)
                {
                    // Si el rol no existe, eliminar el usuario creado
                    await _userRepository.DeleteUserAsync(internalUser);
                    throw new Exception("Role 'Interno' does not exist. User creation has been rolled back.");
                }

                // Normalizar el clientUri para evitar barras diagonales al final
                var normalizedClientUri = registerInternalUserDto.ClientUri.TrimEnd('/');

                // Generar el token de confirmación de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(internalUser);
                var confirmLink = $"{registerInternalUserDto.ClientUri}?userId={internalUser.Id}&token={Uri.EscapeDataString(token)}";

                // Enviar correo de confirmación
                var subject = "Email Confirmation";
                var body = $"<p>To confirm your email address, click the link below:</p><p><a href='{confirmLink}'>Confirm Email</a></p>";
                await _emailService.SendEmailAsync(internalUser.Email, subject, body);

                return true;
            }
            catch (PasswordMismatchException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Loguear el error completo (incluyendo el InnerException si existe)
                throw new Exception("An error occurred while registering the internal user. Details: " + ex.Message, ex);
            }
        }

        public async Task<bool> RegisterExternalUserAsync(RegisterExternalUserDto registerExternalUserDto)
        {
            try
            {
                // Verificación de contraseñas
                if (registerExternalUserDto.Password != registerExternalUserDto.ConfirmPassword)
                    throw new PasswordMismatchException();

                // Usar la fábrica para crear el usuario externo
                var externalUser = _externalUserFactory.CreateUser(
                    registerExternalUserDto.Email,
                    registerExternalUserDto.FirstName,
                    registerExternalUserDto.LastName,
                    registerExternalUserDto.Password,
                    registerExternalUserDto.Cuil,
                    registerExternalUserDto.Empresa,
                    registerExternalUserDto.Descripcion
                );

                // Verificar si el rol "Externo" existe
                var roleExists = await _userRepository.RoleExistsAsync("Externo");
                if (!roleExists)
                {
                    // Si el rol no existe, eliminar el usuario creado
                    await _userRepository.DeleteUserAsync(externalUser);
                    throw new Exception("Role 'Externo' does not exist. User creation has been rolled back.");
                }

                // Normalizar el clientUri para evitar barras diagonales al final
                var normalizedClientUri = registerExternalUserDto.ClientUri.TrimEnd('/');
                // Generar el token de confirmación de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(externalUser);
                var confirmLink = $"{registerExternalUserDto.ClientUri}?userId={externalUser.Id}&token={Uri.EscapeDataString(token)}";

                // Enviar correo de confirmación
                var subject = "Email Confirmation";
                var body = $"<p>To confirm your email address, click the link below:</p><p><a href='{confirmLink}'>Confirm Email</a></p>";
                await _emailService.SendEmailAsync(externalUser.Email, subject, body);

                return true;
            }
            catch (PasswordMismatchException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while registering the external user. Details: " + ex.Message, ex);
            }
        }


        // Método para enviar un enlace de recuperación de contraseña (sin cambios)
        public async Task<bool> SendPasswordResetLinkAsync(string email, string clientUri)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                    throw new UserNotFoundException();

                var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                    throw new PasswordResetException("Failed to generate reset token.");

                // Normalizar el clientUri para evitar barras diagonales al final
                var normalizedClientUri = clientUri.TrimEnd('/');

                // Codificar el token para uso seguro en la URL
                var encodedToken = Uri.EscapeDataString(token);
                var userId = user.Id;

                // Generar el enlace de restablecimiento de contraseña
                var resetLink = $"{normalizedClientUri}?userId={userId}&token={encodedToken}";

                var subject = "Password Reset Request";
                var body = $"<p>To reset your password, click the link below:</p><p><a href='{resetLink}'>Reset Password</a></p>";

                try
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                }
                catch (Exception ex)
                {
                    throw new EmailSendException("An error occurred while sending the email for password reset.", ex);
                }

                return true;
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while processing the password reset request.");
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            // Validar si las contraseñas coinciden
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                throw new InvalidOperationException("The new password and confirmation password do not match.");
            }

            // Buscar al usuario por su UserId a través del repositorio
            var user = await _userRepository.GetUserByIdAsync(resetPasswordDto.UserId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            // Decodificar el token
            var decodedToken = Uri.UnescapeDataString(resetPasswordDto.Token);

            // Intentar restablecer la contraseña
            var result = await _userRepository.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);

            // Verificar si la operación fue exitosa
            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                // Aquí puedes extraer los errores específicos de Identity y devolverlos como detalles
                var errorMessages = result.Errors.Select(e => e.Description).ToList();
                throw new InvalidOperationException($"Failed to reset password. Errors: {string.Join(", ", errorMessages)}");
            }
        }



        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            // Cargar el usuario desde el repositorio
            var user = await _userRepository.GetUserWithRelationsByIdAsync(userId);

            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            // Crear el DTO correspondiente basado en el tipo de usuario
            if (user.AdminUser != null)
            {
                return new AdminUserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    IsEnabled = user.IsEnabled,
                    Image = user.Image,
                    AdminSpecificAttribute = "SomeAdminData",
                    Role = "admin"
                };
            }
            else if (user.InternalUser != null)
            {
                return new InternalUserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    IsEnabled = user.IsEnabled,
                    Image = user.Image,
                    Cargo = user.InternalUser.Cargo,
                    Departamento = user.InternalUser.Departamento,
                    Role = "Interno"
                };
            }
            else if (user.ExternalUser != null)
            {
                return new ExternalUserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    IsEnabled = user.IsEnabled,
                    Image = user.Image,
                    Cuil = user.ExternalUser.Cuil,
                    Empresa = user.ExternalUser.Empresa,
                    Descripcion = user.ExternalUser.Descripcion,
                    Role = "Externo"
                };
            }

            // Si no tiene roles específicos, devolver datos básicos
            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                IsEnabled = user.IsEnabled,
                Image = user.Image
            };
        }

        public async Task<List<UserProfileDto>> GetAllUsersAsync()
        {
            // Obtener todos los usuarios (excepto administradores)
            var users = await _userRepository.GetUsersExcludingAdminsAsync();

            // Usar GetUserProfileAsync para mapear a los DTOs de cada usuario
            var userDtos = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var userProfile = await GetUserProfileAsync(user.Id); // Llamada al método existente para obtener el perfil
                userDtos.Add(userProfile);
            }

            return userDtos;
        }

        // Obtener todos los usuarios externos habilitados
        public async Task<List<ExternalUserProfileDto>> GetExternalUsersAsync()
        {
            // Obtener usuarios externos habilitados desde el repositorio
            var externalUsers = await _userRepository.GetExternalUsersAsync();

            // Mapear a DTO de usuarios externos
            var userDtos = externalUsers.Select(u => new ExternalUserProfileDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                UserName = u.UserName,
                IsEnabled = u.IsEnabled,
                Image = u.Image,
                Cuil = u.ExternalUser?.Cuil,
                Empresa = u.ExternalUser?.Empresa,
                Descripcion = u.ExternalUser?.Descripcion
            }).ToList();

            return userDtos;
        }

        // Obtener todos los usuarios internos habilitados
        public async Task<List<InternalUserProfileDto>> GetInternalUsersAsync()
        {
            // Obtener usuarios internos habilitados desde el repositorio
            var internalUsers = await _userRepository.GetInternalUsersAsync();

            // Mapear a DTO de usuarios internos
            var userDtos = internalUsers.Select(u => new InternalUserProfileDto
            {   
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                UserName = u.UserName,
                IsEnabled = u.IsEnabled,
                Image = u.Image,
                Cargo = u.InternalUser?.Cargo,
                Departamento = u.InternalUser?.Departamento
            }).ToList();

            return userDtos;
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isEnabled)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsEnabled = isEnabled;
            var result = await _userRepository.UpdateUserAsync(user);

            // Si el usuario ha sido activado, enviamos el correo
            if (result && user.IsEnabled)
            {
                await SendAccountActivationEmailAsync(user.Email);
            }

            return result;
        }

        public async Task SendAccountActivationEmailAsync(string email)
        {
            string subject = "Your account has been activated";
            string message = "<p>Your account has been successfully activated. You can now log in.</p>";
            await _emailService.SendEmailAsync(email, subject, message);
        }


        // Subir la imagen para un usuario
        public async Task<User> UploadImageAsync(IFormFile file, string userId)
        {
            // Subir la imagen usando el servicio de imágenes
            var imageUrl = await _imageService.UploadImageAsync(file);
            return await _userRepository.UpdateUserImageAsync(userId, imageUrl);
        }

        // Asignar una imagen predeterminada a los usuarios que no tienen imagen
        public async Task AssignDefaultImageToAllUsersAsync()
        {
            await _userRepository.AssignDefaultImageToAllUsersAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

    }
}
