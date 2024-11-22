namespace trackit.server.Exceptions
{
    public class PasswordMismatchException : Exception
    {
        public PasswordMismatchException() : base("Passwords do not match") { }
    }

}
