namespace PluginContracts;

/// <summary>
/// Интерфейс сервиса событий
/// </summary>
public interface IHelloEventService
{
    /// <summary>
    /// Событие таймера
    /// </summary>
    event EventHandler<TimerEventArgs> TimerEvent;
    
    /// <summary>
    /// Подписаться на событие таймера
    /// </summary>
    void SubscribeToTimer(EventHandler<TimerEventArgs> handler);
    
    /// <summary>
    /// Отписаться от события таймера
    /// </summary>
    void UnsubscribeFromTimer(EventHandler<TimerEventArgs> handler);
}

/// <summary>
/// Аргументы события таймера
/// </summary>
public class TimerEventArgs : EventArgs
{
    public string Message { get; }
    public DateTime Timestamp { get; }
    
    public TimerEventArgs(string message, DateTime timestamp)
    {
        Message = message;
        Timestamp = timestamp;
    }
}
