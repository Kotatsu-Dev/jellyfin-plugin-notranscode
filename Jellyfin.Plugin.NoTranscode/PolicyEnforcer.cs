using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NoTranscode;

/// <summary>
/// Стартовый проход по всем существующим пользователям.
/// Новые пользователи обрабатываются отдельно — см. <see cref="UserCreatedConsumer"/>.
/// </summary>
public class PolicyEnforcer : IHostedService
{
    private readonly IUserManager _userManager;
    private readonly PolicyApplier _applier;
    private readonly ILogger<PolicyEnforcer> _logger;

    public PolicyEnforcer(IUserManager userManager, PolicyApplier applier, ILogger<PolicyEnforcer> logger)
    {
        _userManager = userManager;
        _applier = applier;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.ApplyOnStartup)
        {
            return;
        }

        try
        {
            foreach (var user in _userManager.GetUsers())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _applier.ApplyAsync(user).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Падение плагина на старте не должно ронять сервер.
            _logger.LogError(ex, "NoTranscode: стартовый проход по пользователям не завершился");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
