using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.NoTranscode.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>Запретить перекодирование видео (основная нагрузка на CPU/GPU).</summary>
    public bool DisableVideoTranscoding { get; set; } = true;

    /// <summary>Запретить перекодирование аудио. По умолчанию выключено:
    /// AC3/DTS-дорожки иначе просто не заиграют, а стоит аудио-транскод дёшево.</summary>
    public bool DisableAudioTranscoding { get; set; }

    /// <summary>Запретить remux (смену контейнера без переэнкода). По умолчанию выключено —
    /// remux почти бесплатен и спасает MKV на клиентах, которые его не переваривают.</summary>
    public bool DisableRemuxing { get; set; }

    /// <summary>Пройтись по всем существующим пользователям при старте сервера.</summary>
    public bool ApplyOnStartup { get; set; } = true;

    /// <summary>Применять политику к новым пользователям сразу после создания.</summary>
    public bool ApplyToNewUsers { get; set; } = true;

    /// <summary>Не трогать администраторов.</summary>
    public bool SkipAdministrators { get; set; }
}
