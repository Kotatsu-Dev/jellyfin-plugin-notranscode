# jellyfin-plugin-notranscode

Снимает флаги перекодирования в политике пользователей, оставляя только Direct Play.
Работает на старте сервера и на создание каждого нового пользователя — то, чего в самом
Jellyfin нет (нет шаблона политики для новых аккаунтов).

## Целевая версия

Собрано под **Jellyfin 12.0** (`net10.0`, пакеты `Jellyfin.*` 12.0.0).

Для других версий правь `TargetFramework` и версии пакетов в csproj:

| Jellyfin | TargetFramework | Пакеты    |
|----------|-----------------|-----------|
| 12.0.x   | net10.0         | 12.0.*    |
| 10.11.x  | net9.0          | 10.11.*   |
| 10.10.x  | net8.0          | 10.10.*   |

Для 10.11 и старше также придётся вернуть подписку на событие `IUserManager.OnUserCreated`
вместо `IEventConsumer<UserCreatedEventArgs>` — см. ниже.

## Сборка

Нужен .NET SDK 10 (для Jellyfin 12.0).

```bash
dotnet publish -c Release
```

Результат — `Jellyfin.Plugin.NoTranscode/bin/Release/net10.0/publish/`:
`Jellyfin.Plugin.NoTranscode.dll` и `meta.json`.

## Установка

Плагины лежат в подпапке `plugins` внутри конфига Jellyfin. При запуске в Docker с
смонтированным конфигом это делается прямо на хосте:

```bash
mkdir -p /home/kotatsu-admin/jellyfin-docker/config/plugins/NoTranscode_1.0.0.0
cp Jellyfin.Plugin.NoTranscode.dll meta.json \
   /home/kotatsu-admin/jellyfin-docker/config/plugins/NoTranscode_1.0.0.0/
sudo docker restart jellyfin
sudo docker logs jellyfin -f
```

`meta.json` не обязателен — без него Jellyfin соберёт манифест сам по имени папки
`Имя_Версия`, но тогда GUID плагина в манифесте не совпадёт с GUID в коде. Проще положить.

После рестарта плагин появится в Dashboard → Plugins → My Plugins, настройки — по клику
на него.

## Как устроено

* `PolicyApplier` — общая логика: читает текущую политику через `IUserManager.GetUserDto`,
  снимает разрешённые к снятию флаги и вызывает `UpdatePolicyAsync` **только если**
  что-то реально изменилось. Из-за этой проверки повторный рестарт сервера не пишет
  в лог те же строки второй раз.
* `PolicyEnforcer` — `IHostedService`, стартовый проход по `IUserManager.GetUsers()`.
  Запускается после миграций базы, так что пользователи уже доступны.
* `UserCreatedConsumer` — `IEventConsumer<UserCreatedEventArgs>`. В Jellyfin 12 у
  `IUserManager` больше нет события `OnUserCreated`; `UserManager.CreateUserAsync`
  публикует `UserCreatedEventArgs` через `IEventManager`, а тот резолвит консьюмеров
  из отдельного DI-scope. Поэтому консьюмер регистрируется как `AddScoped`.

## Что менялось под Jellyfin 12

* `Jellyfin.Data.Entities.User` → `Jellyfin.Database.Implementations.Entities.User`.
* `IUserManager.Users` (свойство) → `IUserManager.GetUsers()`.
* `IUserManager.OnUserCreated` удалено → `IEventConsumer<UserCreatedEventArgs>`.
* Нужны отдельные `PackageReference` на `Jellyfin.Data` и `Jellyfin.Database.Implementations`:
  `Jellyfin.Controller` их транзитивно не тянет, хотя типы из них торчат в его публичном API.
* `UserPolicy`, `UserDto.Policy`, `UpdatePolicyAsync(Guid, UserPolicy)` и
  `IPluginServiceRegistrator` — без изменений.

## Что плагин НЕ делает

Не блокирует транскод на уровне сервера — он только выставляет пользовательские флаги,
те же, что руками ставятся в Dashboard → Users → Media Playback. Админ по-прежнему может
вернуть галочку вручную; при следующем рестарте сервера плагин снимет её обратно
(если включено «Применять ко всем при старте»).

## Лицензия

GPL-3.0-only — как и сам Jellyfin, на пакеты которого плагин ссылается.
