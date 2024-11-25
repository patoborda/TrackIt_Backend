using Microsoft.EntityFrameworkCore;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly UserDbContext _context;

        public NotificationRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserNotificationAsync(string userId, int notificationId)
        {
            var existingNotification = await _context.UserNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(un => un.UserId == userId && un.NotificationId == notificationId);

            if (existingNotification == null)
            {
                var userNotification = new UserNotification
                {
                    UserId = userId,
                    NotificationId = notificationId,
                    IsRead = false
                };

                _context.UserNotifications.Add(userNotification);
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"UserNotification already exists for UserId={userId}, NotificationId={notificationId}");
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page, int size)
        {
            return await _context.UserNotifications
                .Where(un => un.UserId == userId)
                .Include(un => un.Notification) // Asegura que se incluyan los datos de la notificación
                .Select(un => un.Notification)
                .OrderByDescending(n => n.Timestamp)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(string userId, int notificationId)
        {
            var userNotification = await _context.UserNotifications
                .FirstOrDefaultAsync(un => un.UserId == userId && un.NotificationId == notificationId);

            if (userNotification != null)
            {
                userNotification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"No UserNotification found for UserId={userId}, NotificationId={notificationId}");
            }
        }

        public async Task<UserNotification?> GetUserNotificationAsync(string userId, int notificationId)
        {
            return await _context.UserNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(un => un.UserId == userId && un.NotificationId == notificationId);
        }

    }
}
