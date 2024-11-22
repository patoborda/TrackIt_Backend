using trackit.server.Models;

namespace trackit.server.Services
{
    public interface IRequirementObserver
    {
        void Update(Requirement requirement, string action, string performedBy, string details);
    }
}
