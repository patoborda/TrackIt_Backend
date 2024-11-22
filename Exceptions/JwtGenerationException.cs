namespace trackit.server.Exceptions
{
    public class JwtGenerationException : Exception
    {
        public JwtGenerationException() : base("An error occurred while generating the JWT token") { }
    }

}
