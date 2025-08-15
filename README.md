# Plugin System

Система плагинов на .NET 7 с архитектурой хост-приложения и подключаемых модулей.

## Описание проекта

Данный проект демонстрирует реализацию архитектуры плагинов в .NET, состоящей из:

- **HostApp** - главное приложение-хост, которое управляет сервисами и загружает плагины
- **PluginContracts** - библиотека контрактов (интерфейсов) для плагинов и сервисов
- **TestPlugin** - пример плагина, демонстрирующий взаимодействие с сервисами

## Архитектура

### Сервисы
- **HelloEventService** (`IHelloEventService`) - сервис событий с таймером (срабатывает каждые 3 секунды)
- **WorldEventService** (`IWorldEventService`) - сервис World данных с таймером (срабатывает каждые 5 секунд)

### Плагины
- **TestPlugin** - демонстрационный плагин, который подписывается на события обоих сервисов

## Структура проекта

```
PluginSystem/
├── HostApp/                    # Главное приложение
│   ├── HelloEventService.cs    # Реализация сервиса событий
│   ├── WorldEventService.cs    # Реализация сервиса World
│   ├── PluginManager.cs        # Менеджер плагинов
│   ├── Program.cs              # Точка входа
│   └── appsettings.json        # Конфигурация
├── PluginContracts/            # Контракты
│   ├── IHelloEventService.cs   # Интерфейс сервиса событий
│   ├── IWorldEventService.cs   # Интерфейс сервиса World
│   └── IPlugin.cs              # Интерфейс плагина
└── TestPlugin/                 # Тестовый плагин
    └── TestPlugin.cs           # Реализация плагина
```

## Требования

- .NET 7.0 SDK или выше
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider

## Команды сборки

### Сборка всех проектов
```bash
# Из корневого каталога
dotnet build PluginContracts/PluginContracts.csproj
dotnet build HostApp/HostApp.csproj
dotnet build TestPlugin/TestPlugin.csproj
```

### Или сборка по отдельности
```bash
# Контракты (базовая библиотека)
dotnet build PluginContracts/PluginContracts.csproj

# Главное приложение
dotnet build HostApp/HostApp.csproj

# Плагин
dotnet build TestPlugin/TestPlugin.csproj
```

## Команды запуска

### Запуск приложения
```bash
# Из корневого каталога
cd HostApp
dotnet run

# Или указав проект явно
dotnet run --project HostApp/HostApp.csproj
```

### Запуск в режиме разработки
```bash
cd HostApp
dotnet run --configuration Debug
```

### Запуск скомпилированного приложения
```bash
cd HostApp/bin/Debug/net7.0
./HostApp.exe
```

## Конфигурация

Настройки приложения находятся в файле `HostApp/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Plugins": {
    "TestPlugin": {
      "Name": "MyCustomTestPlugin",
      "Enabled": true
    }
  }
}
```

## Особенности реализации

### Dependency Injection (DI)
- Все сервисы регистрируются в DI контейнере
- Плагины получают доступ к сервисам через `IServiceProvider`
- Поддерживается жизненный цикл Singleton для сервисов

### Система событий
- **HelloEventService** генерирует события каждые 3 секунды с сообщением "Hello"
- **WorldEventService** генерирует события каждые 5 секунд с случайными World сообщениями
- Плагины могут подписываться на события через интерфейсы сервисов

### Логирование
- Используется встроенная система логирования .NET
- Поддерживается консольное логирование и логирование в Event Log (Windows)
- Настраивается через `appsettings.json`

## Расширение системы

### Создание нового плагина
1. Создайте новый проект Class Library
2. Добавьте ссылку на `PluginContracts`
3. Реализуйте интерфейс `IPlugin`
4. Скомпилируйте и поместите DLL в папку с HostApp

### Добавление нового сервиса
1. Добавьте интерфейс в `PluginContracts`
2. Реализуйте сервис в `HostApp`
3. Зарегистрируйте в DI в `Program.cs`
4. Используйте в плагинах через `IServiceProvider`

## Лицензия

Этот проект создан в учебных целях и доступен для свободного использования.
