using System;

namespace trackit.server.Exceptions
{
    public class InvalidLoginException : Exception
    {
        public InvalidLoginException() : base("Invalid login credentials")
        {
        }

        public InvalidLoginException(string message) : base(message)
        {
        }

        public InvalidLoginException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
