using System;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NoTranscode;

/// <summary>
/// Приводит политику одного пользователя к состоянию "только Direct Play".
/// Общая логика для стартового прохода и для обработчика создания пользователя.
/// </summary>
public class PolicyApplier
{
    private readonly IUserManager _userManager;
    private readonly ILogger<PolicyApplier> _logger;

    public PolicyApplier(IUserManager userManager, ILogger<PolicyApplier> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task ApplyAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var config = Plugin.Instance?.Configuration;
        if (config is null)
        {
            return;
        }

        var policy = _userManager.GetUserDto(user).Policy;
        if (policy is null)
        {
            return;
        }

        if (config.SkipAdministrators && policy.IsAdministrator)
        {
            return;
        }

        // Пишем только при реальном изменении — иначе получим лишние события обновления.
        var changed = false;

        if (config.DisableVideoTranscoding && policy.EnableVideoPlaybackTranscoding)
        {
            policy.EnableVideoPlaybackTranscoding = false;
            changed = true;
        }

        if (config.DisableAudioTranscoding && policy.EnableAudioPlaybackTranscoding)
        {
            policy.EnableAudioPlaybackTranscoding = false;
            changed = true;
        }

        if (config.DisableRemuxing && policy.EnablePlaybackRemuxing)
        {
            policy.EnablePlaybackRemuxing = false;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        await _userManager.UpdatePolicyAsync(user.Id, policy).ConfigureAwait(false);
        _logger.LogInformation("NoTranscode: политика обновлена для пользователя {User}", user.Username);
    }
}
