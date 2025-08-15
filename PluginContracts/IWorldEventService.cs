namespace PluginContracts;

/// <summary>
/// Интерфейс сервиса для работы с "World" данными
/// </summary>
public interface IWorldEventService
{
    /// <summary>
    /// Событие когда данные World обновляются
    /// </summary>
    event EventHandler<WorldEventArgs> WorldUpdated;
    
    /// <summary>
    /// Получить текущее значение World
    /// </summary>
    string GetWorldMessage();
    
    /// <summary>
    /// Подписаться на обновления World
    /// </summary>
    void SubscribeToWorldUpdates(EventHandler<WorldEventArgs> handler);
    
    /// <summary>
    /// Отписаться от обновлений World
    /// </summary>
    void UnsubscribeFromWorldUpdates(EventHandler<WorldEventArgs> handler);
}

/// <summary>
/// Аргументы события обновления World
/// </summary>
public class WorldEventArgs : EventArgs
{
    public string WorldMessage { get; }
    public DateTime Timestamp { get; }
    
    public WorldEventArgs(string worldMessage, DateTime timestamp)
    {
        WorldMessage = worldMessage;
        Timestamp = timestamp;
    }
}
