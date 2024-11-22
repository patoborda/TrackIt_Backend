using System.Collections.Generic;
using trackit.server.Models;

namespace trackit.server.Services
{
    public class RequirementNotifier
    {
        private readonly List<IRequirementObserver> _observers = new();

        public void Attach(IRequirementObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IRequirementObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers(Requirement requirement, string action, string performedBy, string details)
        {
            foreach (var observer in _observers)
            {
                observer.Update(requirement, action, performedBy, details);
            }
        }
    }
}
