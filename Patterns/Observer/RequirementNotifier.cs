using System.Collections.Generic;
using System.Threading.Tasks;
using trackit.server.Patterns.Observer;

namespace trackit.server.Patterns.Observer
{
    public class RequirementNotifier
    {
        private readonly List<IObserver> _observers = new();

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyAllAsync(string message, object data)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.NotifyAsync(message, data);
                    Console.WriteLine($"Notification sent to observer: {observer.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying observer {observer.GetType().Name}: {ex.Message}");
                }
            }
        }


    }
}
