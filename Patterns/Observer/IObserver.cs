namespace trackit.server.Patterns.Observer
{
    public interface IObserver
    {
        Task NotifyAsync(string message, object data);
    }
}
