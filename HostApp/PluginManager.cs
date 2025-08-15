using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using PluginContracts;

namespace HostApp;

/// <summary>
/// Менеджер плагинов
/// </summary>
public class PluginManager
{
    private readonly ILogger<PluginManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IPlugin> _loadedPlugins = new();
    
    public PluginManager(ILogger<PluginManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Загружает плагин из указанного файла
    /// </summary>
    public void LoadPlugin(string pluginPath, string? configurationName = null)
    {
        try
        {
            _logger.LogInformation("Загрузка плагина из: {PluginPath}", pluginPath);
            
            // Загружаем сборку
            var assembly = Assembly.LoadFrom(pluginPath);
            _logger.LogInformation("Сборка загружена: {AssemblyName}", assembly.FullName);
            
            // Ищем типы, реализующие интерфейс IPlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && 
                           t.GetMethod("Initialize") != null &&
                           t.GetProperty("Name") != null)
                .ToList();
            
            _logger.LogInformation("Найдено типов плагинов: {Count}", pluginTypes.Count);
            
            foreach (var pluginType in pluginTypes)
            {
                _logger.LogInformation("Создание экземпляра плагина: {PluginType}", pluginType.FullName);
                try
                {
                    // Создаем экземпляр плагина
                    var pluginInstance = CreatePluginInstance(pluginType);
                    
                    // Создаем адаптер для плагина
                    var plugin = new PluginAdapter(pluginInstance, pluginType, configurationName);
                    
                    // Устанавливаем имя из конфигурации в сам плагин, если оно указано
                    if (!string.IsNullOrEmpty(configurationName))
                    {
                        plugin.SetConfigurationName(configurationName);
                    }
                    
                    // Регистрируем плагин
                    RegisterPlugin(plugin);
                    
                    // Создаем scope для плагина и инициализируем
                    using var scope = _serviceProvider.CreateScope();
                    plugin.Initialize(scope.ServiceProvider);
                    
                    _logger.LogInformation("Плагин '{ConfigName}' (тип: {PluginType}) успешно загружен и инициализирован", 
                        plugin.ConfigurationName ?? "без имени", plugin.PluginTypeName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании экземпляра плагина типа {PluginType}", pluginType.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке плагина из {PluginPath}", pluginPath);
        }
    }

    /// <summary>
    /// Создает экземпляр плагина используя DI контейнер
    /// </summary>
    private object CreatePluginInstance(Type pluginType)
    {
        // Получаем конструктор плагина
        var constructor = pluginType.GetConstructors().FirstOrDefault();
        if (constructor == null)
        {
            throw new InvalidOperationException($"Не найден публичный конструктор для типа {pluginType.FullName}");
        }

        var parameters = constructor.GetParameters();
        var constructorArgs = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            
            // Пытаемся получить сервис из DI контейнера
            try
            {
                var service = _serviceProvider.GetService(paramType);
                if (service != null)
                {
                    constructorArgs[i] = service;
                }
                else
                {
                    throw new InvalidOperationException($"Не удалось получить сервис типа {paramType.FullName} из DI контейнера");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении сервиса типа {paramType.FullName}: {ex.Message}", ex);
            }
        }

        return Activator.CreateInstance(pluginType, constructorArgs)!;
    }
    
    /// <summary>
    /// Регистрирует плагин в системе
    /// </summary>
    private void RegisterPlugin(IPlugin plugin)
    {
        var pluginAdapter = plugin as PluginAdapter;
        if (pluginAdapter != null)
        {
            _logger.LogInformation("Регистрация плагина: '{ConfigName}' (тип: {PluginType})", 
                pluginAdapter.ConfigurationName ?? "без имени", pluginAdapter.PluginTypeName);
        }
        else
        {
            _logger.LogInformation("Регистрация плагина: {PluginName}", plugin.Name);
        }
        
        _loadedPlugins.Add(plugin);
    }
    
    /// <summary>
    /// Получает список загруженных плагинов
    /// </summary>
    public IReadOnlyList<IPlugin> GetLoadedPlugins() => _loadedPlugins.AsReadOnly();
}

/// <summary>
/// Адаптер для плагинов, который реализует интерфейс IPlugin
/// </summary>
public class PluginAdapter : IPlugin
{
    private readonly object _pluginInstance;
    private readonly Type _pluginType;
    
    public PluginAdapter(object pluginInstance, Type pluginType, string? configurationName = null)
    {
        _pluginInstance = pluginInstance;
        _pluginType = pluginType;
        ConfigurationName = configurationName;
    }
    
    /// <summary>
    /// Имя плагина из конфигурации хоста
    /// </summary>
    public string? ConfigurationName { get; }
    
    /// <summary>
    /// Имя типа плагина (внутреннее имя самого плагина)
    /// </summary>
    public string PluginTypeName
    {
        get
        {
            var nameProperty = _pluginType.GetProperty("Name");
            return nameProperty?.GetValue(_pluginInstance)?.ToString() ?? _pluginType.Name;
        }
    }
    
    /// <summary>
    /// Основное имя плагина (для совместимости с интерфейсом IPlugin)
    /// Возвращает имя из конфигурации, если есть, иначе имя типа
    /// </summary>
    public string Name => ConfigurationName ?? PluginTypeName;
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        var initializeMethod = _pluginType.GetMethod("Initialize");
        if (initializeMethod != null)
        {
            var parameters = initializeMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IServiceProvider))
            {
                // Новый способ: плагин принимает IServiceProvider
                initializeMethod.Invoke(_pluginInstance, new object[] { serviceProvider });
            }
            else if (parameters.Length == 0)
            {
                // Старый способ: плагин не принимает параметры
                initializeMethod.Invoke(_pluginInstance, null);
            }
            else
            {
                throw new InvalidOperationException($"Неподдерживаемая сигнатура метода Initialize для плагина {_pluginType.FullName}");
            }
        }
    }
    
    public void SetConfigurationName(string configurationName)
    {
        var setConfigNameMethod = _pluginType.GetMethod("SetConfigurationName");
        setConfigNameMethod?.Invoke(_pluginInstance, new object[] { configurationName });
    }
}
