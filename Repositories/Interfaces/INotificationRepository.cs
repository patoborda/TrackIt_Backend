using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page, int size);
        Task MarkAsReadAsync(string userId, int notificationId); // Ahora incluye UserId
        Task AddUserNotificationAsync(string userId, int notificationId); // Nuevo método para agregar relaciones
    }
}
