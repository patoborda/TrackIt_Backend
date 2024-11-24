using System;

namespace trackit.server.Exceptions
{
    public class EmailNotConfirmedException : Exception
    {
        public EmailNotConfirmedException() : base("Email not confirmed.")
        {
        }

        public EmailNotConfirmedException(string message) : base(message)
        {
        }

        public EmailNotConfirmedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}