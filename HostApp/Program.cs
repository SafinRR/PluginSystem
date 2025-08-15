using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PluginContracts;

namespace HostApp;

class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var pluginManager = host.Services.GetRequiredService<PluginManager>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var eventService = host.Services.GetRequiredService<IHelloEventService>();
        var worldService = host.Services.GetRequiredService<IWorldEventService>();
        
        logger.LogInformation("Приложение с плагинной системой запущено");
        
        try
        {
            // Загружаем плагины из конфигурации
            var pluginSettings = new PluginSettings();
            configuration.GetSection("PluginSettings").Bind(pluginSettings);
            
            if (pluginSettings.Plugins.Any())
            {
                logger.LogInformation("Найдено плагинов в конфигурации: {Count}", pluginSettings.Plugins.Count);
                
                foreach (var pluginConfig in pluginSettings.Plugins)
                {
                    if (!pluginConfig.Enabled)
                    {
                        logger.LogInformation("Плагин {PluginName} отключен в конфигурации", pluginConfig.Name);
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(pluginConfig.Path))
                    {
                        logger.LogWarning("Не указан путь для плагина {PluginName}", pluginConfig.Name);
                        continue;
                    }
                    
                    logger.LogInformation("Загрузка плагина: '{ConfigName}' из {PluginPath}", 
                        pluginConfig.Name, pluginConfig.Path);
                        
                    pluginManager.LoadPlugin(pluginConfig.Path, pluginConfig.Name);
                }
            }
            else
            {
                logger.LogWarning("Плагины не настроены в конфигурации");
            }
            
            // Показываем загруженные плагины
            var loadedPlugins = pluginManager.GetLoadedPlugins();
            logger.LogInformation("Успешно загружено плагинов: {Count}", loadedPlugins.Count);
            
            foreach (var plugin in loadedPlugins)
            {
                var pluginAdapter = plugin as PluginAdapter;
                if (pluginAdapter != null)
                {
                    logger.LogInformation("Загружен плагин: '{ConfigName}' (тип: {PluginType})", 
                        pluginAdapter.ConfigurationName ?? "без имени", pluginAdapter.PluginTypeName);
                }
                else
                {
                    logger.LogInformation("Загружен плагин: {PluginName}", plugin.Name);
                }
            }
            
            // Ждем событий от таймера
            logger.LogInformation("Приложение работает. Ждем события от таймера...");
            logger.LogInformation("Нажмите любую клавишу для завершения...");
            
            // Ждем только нажатия клавиши
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при работе приложения");
        }
        
        logger.LogInformation("Приложение завершается");
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Регистрируем сервисы
                services.AddSingleton<IHelloEventService, HelloEventService>();
                services.AddSingleton<IWorldEventService, WorldEventService>();
                services.AddSingleton<PluginManager>();
                
                // Настраиваем логирование
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}
