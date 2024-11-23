using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public interface IInternalUserFactory
    {
        User CreateUser(string email, string firstName, string lastName, string password, string cargo, string departamento);
    }
}
