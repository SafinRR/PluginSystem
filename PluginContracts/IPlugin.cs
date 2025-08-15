using Microsoft.Extensions.DependencyInjection;

namespace PluginContracts;

/// <summary>
/// Интерфейс для плагинов
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Имя плагина
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Метод инициализации плагина с доступом к сервисам через DI
    /// </summary>
    void Initialize(IServiceProvider serviceProvider);
    
    /// <summary>
    /// Устанавливает имя плагина из конфигурации хоста
    /// </summary>
    void SetConfigurationName(string configurationName);
}
