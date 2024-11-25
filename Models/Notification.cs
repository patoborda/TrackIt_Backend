namespace trackit.server.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false; // La propiedad IsRead ahora será manejada en UserNotification

        // Relación con la tabla intermedia
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}
