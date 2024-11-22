namespace trackit.server.Exceptions
{
    public class JwtKeyNotConfiguredException : Exception
    {
        public JwtKeyNotConfiguredException() : base("JWT Key is not configured in appsettings.json") { }
    }

}
