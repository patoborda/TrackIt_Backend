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
            var userNotification = new UserNotification
            {
                UserId = userId,
                NotificationId = notificationId,
                IsRead = false // Estado inicial
            };

            _context.Add(userNotification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page, int size)
        {
            return await _context.UserNotifications
                .Where(un => un.UserId == userId)
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
        }

    }
}
