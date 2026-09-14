using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.NoTranscode;

/// <summary>
/// Применяет политику к каждому вновь созданному пользователю.
/// В Jellyfin 12 у IUserManager нет события OnUserCreated — вместо него
/// UserManager публикует UserCreatedEventArgs через IEventManager.
/// </summary>
public class UserCreatedConsumer : IEventConsumer<UserCreatedEventArgs>
{
    private readonly PolicyApplier _applier;

    public UserCreatedConsumer(PolicyApplier applier)
    {
        _applier = applier;
    }

    public async Task OnEvent(UserCreatedEventArgs eventArgs)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.ApplyToNewUsers)
        {
            return;
        }

        await _applier.ApplyAsync(eventArgs.Argument).ConfigureAwait(false);
    }
}
