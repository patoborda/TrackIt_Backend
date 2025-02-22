using trackit.server.Services.Interfaces;

namespace trackit.server.Patterns.Observer
{
    public class EmailNotificationObserver : IObserver
    {
        private readonly IEmailService _emailService;

        public EmailNotificationObserver(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task NotifyAsync(string message, object data)
        {
            if (data is EmailNotificationData emailData)
            {
                // Crear el templateData para pasarlo al método SendEmailAsync
                var templateData = new { Content = emailData.Content };

                // Pasar templateData al método SendEmailAsync
                await _emailService.SendEmailAsync(emailData.Email, "Notification", $"{message}: {emailData.Content}", templateData);
            }
            else
            {
                Console.WriteLine("Invalid data type for EmailNotificationObserver.");
            }
        }
    }

    public class EmailNotificationData
    {
        public required string Email { get; set; }
        public required string Content { get; set; }
    }
}
