using Microsoft.Extensions.Logging;
using PluginContracts;

namespace HostApp;

/// <summary>
/// Реализация сервиса событий
/// </summary>
public class HelloEventService : IHelloEventService, IDisposable
{
    private readonly ILogger<HelloEventService> _logger;
    private readonly Timer _timer;
    private readonly List<EventHandler<TimerEventArgs>> _subscribers = new();
    
    public event EventHandler<TimerEventArgs>? TimerEvent;
    
    public HelloEventService(ILogger<HelloEventService> logger)
    {
        _logger = logger;
        
        // Создаем таймер, который срабатывает каждые 3 секунды
        _timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
        _logger.LogInformation("HelloEventService создан, таймер запущен");
    }
    
    private void OnTimer(object? state)
    {
        var args = new TimerEventArgs("Hello", DateTime.Now);
        _logger.LogInformation("Таймер сработал: {Message} в {Timestamp}", args.Message, args.Timestamp);
        
        // Вызываем событие
        TimerEvent?.Invoke(this, args);
        
        // Уведомляем всех подписчиков
        foreach (var subscriber in _subscribers)
        {
            try
            {
                subscriber(this, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при уведомлении подписчика");
            }
        }
    }
    
    public void SubscribeToTimer(EventHandler<TimerEventArgs> handler)
    {
        _subscribers.Add(handler);
        _logger.LogInformation("Добавлен новый подписчик на событие таймера");
    }
    
    public void UnsubscribeFromTimer(EventHandler<TimerEventArgs> handler)
    {
        _subscribers.Remove(handler);
        _logger.LogInformation("Удален подписчик с события таймера");
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        _logger.LogInformation("HelloEventService остановлен");
    }
}
