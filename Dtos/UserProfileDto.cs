﻿namespace trackit.server.Dtos
{
    public class UserProfileDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public bool IsEnabled { get; set; }

    }

}
