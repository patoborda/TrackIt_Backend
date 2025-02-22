namespace trackit.server.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string TemplateName, object? templateData);
    }
}