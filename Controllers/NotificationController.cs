using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Repositories.Interfaces;
using trackit.server.Dtos;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(
            string userId,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID cannot be null or empty." });

            if (page < 1 || size < 1)
                return BadRequest(new { message = "Page and size must be greater than zero." });

            try
            {
                // Fetch paginated notifications
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, size);

                if (notifications == null || !notifications.Any())
                    return NotFound(new { message = $"No notifications found for user with ID: {userId}" });

                // Map to DTOs
                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    Timestamp = n.Timestamp,
                    IsRead = n.IsRead
                });

                return Ok(notificationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving notifications: {ex.Message}" });
            }
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(string userId, int notificationId)
        {
            if (string.IsNullOrEmpty(userId) || notificationId <= 0)
            {
                return BadRequest(new { message = "Invalid user ID or notification ID." });
            }

            try
            {
                await _notificationRepository.MarkAsReadAsync(userId, notificationId);
                return Ok(new { message = "Notification marked as read successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error marking notification as read: {ex.Message}" });
            }
        }

    }
}
