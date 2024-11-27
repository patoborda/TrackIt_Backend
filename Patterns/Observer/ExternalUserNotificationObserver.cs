using trackit.server.Models;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Patterns.Observer
{
    public class ExternalNotificationObserver : IObserver
    {
        private readonly INotificationRepository _notificationRepository;

        public ExternalNotificationObserver(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task NotifyAsync(string message, object data)
        {
            try
            {
                if (data is ExternalNotificationData notificationData)
                {
                    var notification = new Notification
                    {
                        Message = $"{message}: {notificationData.Content}",
                        Timestamp = DateTime.UtcNow
                    };

                    // Crear la notificación en la base de datos
                    await _notificationRepository.AddNotificationAsync(notification);

                    // Crear la relación UserNotification para cada usuario externo
                    await _notificationRepository.AddUserNotificationAsync(notificationData.UserId, notification.Id);

                    Console.WriteLine($"External notification created for UserId: {notificationData.UserId}");
                }
                else
                {
                    Console.WriteLine("Invalid data provided for external notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving external notification: {ex.Message}");
                throw;
            }
        }
    }

    public class ExternalNotificationData
    {
        public required string UserId { get; set; } // ID del usuario externo
        public required string Content { get; set; } // Contenido del mensaje
    }
}
