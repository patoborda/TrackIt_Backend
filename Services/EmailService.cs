using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using trackit.server.Services.Interfaces;
using trackit.server.Exceptions;
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
        public async Task SendEmailAsync(string to, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();

                // Remitente
                emailMessage.From.Add(new MailboxAddress("Your App", _configuration["Email:FromAddress"]));

                // Destinatario
                emailMessage.To.Add(new MailboxAddress(string.Empty, to));

                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    // Intenta convertir el puerto SMTP de la configuración a un número entero
                    if (int.TryParse(_configuration["Email:SmtpPort"], out int smtpPort))
                    {
                        // Conectar al servidor SMTP con STARTTLS usando el puerto validado
                        await smtpClient.ConnectAsync(_configuration["Email:SmtpHost"], smtpPort, SecureSocketOptions.StartTls);

                        // Autenticación
                        await smtpClient.AuthenticateAsync(_configuration["Email:FromAddress"], _configuration["Email:FromPassword"]);

                        // Enviar el mensaje
                        await smtpClient.SendAsync(emailMessage);

                        // Desconectar
                        await smtpClient.DisconnectAsync(true);
                    }
                    else
                    {
                        // Manejo de error si el puerto SMTP no se puede convertir a entero
                        throw new InvalidOperationException("El valor de SmtpPort no es un número válido.");
                    }
                }

            }
            catch (SmtpCommandException smtpEx)
            {
                // Esto captura errores específicos de los comandos SMTP
                _logger.LogError(smtpEx, "SMTP Command Error while sending email to {To}", to);
                throw new EmailSendException("SMTP Command Error: " + smtpEx.Message);
            }
            catch (SmtpProtocolException smtpProtocolEx)
            {
                // Errores de protocolo SMTP
                _logger.LogError(smtpProtocolEx, "SMTP Protocol Error while sending email to {To}", to);
                throw new EmailSendException("SMTP Protocol Error: " + smtpProtocolEx.Message);
            }
            catch (Exception ex)
            {
                // Captura cualquier otra excepción general
                _logger.LogError(ex, "Unexpected error while sending email to {To}", to);
                throw new EmailSendException("Unexpected error while sending email: " + ex.Message);
            }
        }

    }
}
