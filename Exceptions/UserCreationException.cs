namespace trackit.server.Exceptions
{
    public class UserCreationException : Exception
    {
        public UserCreationException() : base("Failed to create user") { }
    }

}
