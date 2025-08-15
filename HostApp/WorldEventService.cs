using Microsoft.Extensions.Logging;
using PluginContracts;

namespace HostApp;

/// <summary>
/// Реализация сервиса World
/// </summary>
public class WorldEventService : IWorldEventService, IDisposable
{
    private readonly ILogger<WorldEventService> _logger;
    private readonly Timer _timer;
    private readonly Random _random = new();
    
    public event EventHandler<WorldEventArgs>? WorldUpdated;
    
    private readonly string[] _worldMessages = 
    {
        "World", "Universe", "Galaxy", "Planet", "Earth", "Globe", "Cosmos"
    };
    
    public WorldEventService(ILogger<WorldEventService> logger)
    {
        _logger = logger;
        
        // Создаем таймер, который срабатывает каждые 5 секунд
        _timer = new Timer(OnTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        _logger.LogInformation("WorldEventService создан, таймер запущен");
    }
    
    private void OnTimer(object? state)
    {
        var worldMessage = GetWorldMessage();
        var args = new WorldEventArgs(worldMessage, DateTime.Now);
        _logger.LogInformation("World обновлен: {WorldMessage} в {Timestamp}", args.WorldMessage, args.Timestamp);
        
        // Безопасный вызов события с обработкой ошибок
        SafeInvokeEvent(WorldUpdated, args);
    }
    
    private void SafeInvokeEvent(EventHandler<WorldEventArgs>? eventHandler, WorldEventArgs args)
    {
        if (eventHandler == null) return;
        
        foreach (EventHandler<WorldEventArgs> handler in eventHandler.GetInvocationList())
        {
            try
            {
                handler(this, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при уведомлении подписчика WorldEventService");
            }
        }
    }
    
    public string GetWorldMessage()
    {
        return _worldMessages[_random.Next(_worldMessages.Length)];
    }
    
    public void SubscribeToWorldUpdates(EventHandler<WorldEventArgs> handler)
    {
        WorldUpdated += handler;
        _logger.LogInformation("Добавлен новый подписчик на обновления World");
    }
    
    public void UnsubscribeFromWorldUpdates(EventHandler<WorldEventArgs> handler)
    {
        WorldUpdated -= handler;
        _logger.LogInformation("Удален подписчик с обновлений World");
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        _logger.LogInformation("WorldEventService остановлен");
    }
}
