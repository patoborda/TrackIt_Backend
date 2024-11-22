using System;

namespace trackit.server.Exceptions
{
    // Excepción personalizada para cuando falla la generación del token de restablecimiento de contraseña
    public class PasswordResetException : Exception
    {
        // Constructor que recibe el mensaje de la excepción
        public PasswordResetException(string message)
            : base(message)
        {
        }

        // Constructor que recibe el mensaje y la excepción interna (para excepciones anidadas)
        public PasswordResetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
