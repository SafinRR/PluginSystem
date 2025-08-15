namespace HostApp;

/// <summary>
/// Настройки плагинов
/// </summary>
public class PluginSettings
{
    public List<PluginConfiguration> Plugins { get; set; } = new();
}

/// <summary>
/// Конфигурация отдельного плагина
/// </summary>
public class PluginConfiguration
{
    /// <summary>
    /// Имя плагина
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Путь к сборке плагина
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Включен ли плагин
    /// </summary>
    public bool Enabled { get; set; } = true;
}
