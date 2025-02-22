using HandlebarsDotNet;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using System.Threading.Tasks;
using trackit.server.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Security;

namespace trackit.server.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Implementación correcta del método SendEmailAsync
        public async Task SendEmailAsync(string to, string subject, string templateName, object templateData)
        {
            try
            {
                // Cargar la plantilla .html desde la carpeta Templates
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"{templateName}.html");
      

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template '{templateName}.html' not found.");
                }

                // Leer la plantilla
                string templateSource = await File.ReadAllTextAsync(templatePath);

                // Compilar la plantilla con Handlebars
                var template = Handlebars.Compile(templateSource);

                // Aplicar los datos dinámicos a la plantilla
                var body = template(templateData);

                // Crear el mensaje de correo electrónico
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("TrackIt", _configuration["Email:FromAddress"]));
                emailMessage.To.Add(new MailboxAddress("", to));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };

                emailMessage.Body = bodyBuilder.ToMessageBody();

                // Enviar el correo electrónico a través de SMTP
                using (var smtpClient = new SmtpClient())
                {
                    // Intenta convertir el puerto SMTP de la configuración a un número entero
                    int smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
                    await smtpClient.ConnectAsync(_configuration["Email:SmtpHost"], smtpPort, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_configuration["Email:FromAddress"], _configuration["Email:FromPassword"]);
                    await smtpClient.SendAsync(emailMessage);
                    await smtpClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                throw new Exception("Error sending email", ex);
            }
        }
    }
}