using System;

namespace trackit.server.Exceptions
{
    // Excepción personalizada para errores al enviar el correo electrónico
    public class EmailSendException : Exception
    {
        // Constructor que recibe el mensaje de la excepción
        public EmailSendException(string message)
            : base(message)
        {
        }

        // Constructor que recibe el mensaje y la excepción interna (para excepciones anidadas)
        public EmailSendException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
