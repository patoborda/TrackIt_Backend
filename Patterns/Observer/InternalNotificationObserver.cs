using trackit.server.Models;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Patterns.Observer
{
    public class InternalNotificationObserver : IObserver
    {
        private readonly INotificationRepository _notificationRepository;

        public InternalNotificationObserver(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task NotifyAsync(string message, object data)
        {
            try
            {
                if (data is InternalNotificationData notificationData)
                {
                    var notification = new Notification
                    {
                        Message = $"{message}: {notificationData.Content}",
                        Timestamp = DateTime.UtcNow
                    };

                    // Crear la notificación en la base de datos
                    await _notificationRepository.AddNotificationAsync(notification);

                    // Crear la relación UserNotification para cada usuario
                    foreach (var userId in notificationData.UserIds)
                    {
                        await _notificationRepository.AddUserNotificationAsync(userId, notification.Id);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid data provided for internal notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving internal notification: {ex.Message}");
                throw;
            }
        }
    }

    public class InternalNotificationData
    {
        public List<string> UserIds { get; set; } = new(); // Ahora admite múltiples usuarios
        public required string Content { get; set; }
    }

}
