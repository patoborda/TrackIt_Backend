using System;

namespace trackit.server.Exceptions
{
    public class UserNotEnabledException : Exception
    {
        public UserNotEnabledException() : base("User account is not enabled.")
        {
        }

        public UserNotEnabledException(string message) : base(message)
        {
        }

        public UserNotEnabledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}