namespace trackit.server.Models
{
    public class UserNotification
    {
        public string UserId { get; set; } = string.Empty; // Relación con User
        public User User { get; set; } = null!;

        public int NotificationId { get; set; } // Relación con Notification
        public Notification Notification { get; set; } = null!;

        public bool IsRead { get; set; } = false; // Estado de lectura por usuario
    }
}
