using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page, int size);
        /// <summary>
        /// Agrega una nueva notificación a la base de datos.
        /// </summary>
        Task AddNotificationAsync(Notification notification);

        /// <summary>
        /// Recupera las notificaciones asociadas a un usuario con paginación.
        /// </summary>
        Task<UserNotification?> GetUserNotificationAsync(string userId, int notificationId);

        /// <summary>
        /// Marca una notificación como leída para un usuario específico.
        /// </summary>
        Task MarkAsReadAsync(string userId, int notificationId);

        /// <summary>
        /// Agrega una relación entre un usuario y una notificación.
        /// </summary>
        Task AddUserNotificationAsync(string userId, int notificationId);

    }
}
