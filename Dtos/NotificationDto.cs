using System;
using System.Collections.Generic;


    namespace trackit.server.Dtos
    {
        public class NotificationDto
        {
            public int Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public bool IsRead { get; set; }
        }
    }

