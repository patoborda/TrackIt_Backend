using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public interface IExternalUserFactory
    {
        User CreateUser(string email, string firstName, string lastName, string password, string cuil, string empresa, string descripcion);
    }
}
