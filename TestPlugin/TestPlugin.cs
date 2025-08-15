using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PluginContracts;

namespace TestPlugin;

/// <summary>
/// Тестовый плагин
/// </summary>
public class TestPlugin : IPlugin
{
    private readonly ILogger<TestPlugin> _logger;
    private string? _configurationName;
    
    /// <summary>
    /// Тип плагина (внутреннее имя типа)
    /// </summary>
    public string PluginType => "TestPlugin V1";
    
    /// <summary>
    /// Имя плагина (для совместимости с интерфейсом IPlugin)
    /// Возвращает имя из конфигурации, если есть, иначе тип плагина
    /// </summary>
    public string Name => _configurationName ?? PluginType;
    
    /// <summary>
    /// Имя плагина из конфигурации хоста или тип плагина по умолчанию
    /// </summary>
    public string EffectiveName => _configurationName ?? PluginType;
    
    public TestPlugin(ILogger<TestPlugin> logger)
    {
        _logger = logger;
    }
    
    public void SetConfigurationName(string configurationName)
    {
        _configurationName = configurationName;
        _logger.LogInformation("Плагину установлено имя из конфигурации: '{ConfigName}' (тип: {PluginType})", 
            configurationName, PluginType);
    }
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Плагин '{EffectiveName}' (тип: {PluginType}) инициализирован", EffectiveName, PluginType);
        
        // Получаем сервис событий через DI 
        var eventService = serviceProvider.GetService<IHelloEventService>();
        if (eventService != null)
        {
            _logger.LogInformation("Плагин '{EffectiveName}' (тип: {PluginType}) подписывается на события", EffectiveName, PluginType);
            
            // Подписываемся на событие таймера напрямую через интерфейс
            eventService.SubscribeToTimer(OnTimerEvent);
            
            _logger.LogInformation("Плагин '{EffectiveName}' (тип: {PluginType}) успешно подписан на события", EffectiveName, PluginType);
        }
        else
        {
            _logger.LogWarning("Плагин '{EffectiveName}' (тип: {PluginType}) не может получить сервис событий через DI", EffectiveName, PluginType);
        }
        
        // Получаем WorldEventService через DI - демонстрация множественных сервисов!
        var worldService = serviceProvider.GetService<IWorldEventService>();
        if (worldService != null)
        {
            _logger.LogInformation("Плагин '{EffectiveName}' (тип: {PluginType}) подписывается на World обновления", EffectiveName, PluginType);
            
            // Подписываемся на события World
            worldService.SubscribeToWorldUpdates(OnWorldEvent);
            
            // Получаем текущее значение World
            var currentWorld = worldService.GetWorldMessage();
            _logger.LogInformation("Плагин '{EffectiveName}' получил текущий World: {WorldMessage}", EffectiveName, currentWorld);
            
            _logger.LogInformation("Плагин '{EffectiveName}' (тип: {PluginType}) успешно подписан на World обновления", EffectiveName, PluginType);
        }
        else
        {
            _logger.LogWarning("Плагин '{EffectiveName}' (тип: {PluginType}) не может получить WorldEventService через DI", EffectiveName, PluginType);
        }
    }
    
    private void OnTimerEvent(object? sender, TimerEventArgs e)
    {
        _logger.LogInformation("[{EffectiveName}] Получено событие: {Message} в {Timestamp}", 
            EffectiveName, e.Message, e.Timestamp);
    }
    
    private void OnWorldEvent(object? sender, WorldEventArgs e)
    {
        _logger.LogInformation("[{EffectiveName}] Получено World обновление: {WorldMessage} в {Timestamp}", 
            EffectiveName, e.WorldMessage, e.Timestamp);
    }
}
