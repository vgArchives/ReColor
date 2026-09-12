#if MELONLOADER
using MelonLoader;
using ReColor;
using UnityEngine;

[assembly: MelonInfo(typeof(ReColorPlugin), ReColorPlugin.PluginName, ReColorPlugin.PluginVersion,
    ReColorPlugin.PluginAuthor)]
[assembly: MelonGame("Mandragora", "Restory")]

namespace ReColor;

public partial class ReColorPlugin : MelonMod
{
    private const string GeneralSection = "ReColorGeneral";
    private const string HotkeysSection = "ReColorHotkeys";

    internal static MelonPreferences_Entry<bool> UpdateCheckEnabled;

    private static MelonPreferences_Category _general;
    private static MelonPreferences_Category _hotkeys;

    public override void OnEarlyInitializeMelon()
    {
        Log.Source = LoggerInstance;
    }

    public override void OnInitializeMelon()
    {
        BindConfig();

        RunSelfChecks();

        if (UpdateCheckEnabled.Value)
        {
            MelonCoroutines.Start(UpdateCheck.Run(PluginVersion));
        }
    }

    public override void OnUpdate()
    {
        HandleHotkeys();
    }

    private void BindConfig()
    {
        _general = MelonPreferences.CreateCategory(GeneralSection, $"{PluginName} - General");
        _hotkeys = MelonPreferences.CreateCategory(HotkeysSection, $"{PluginName} - Hotkeys");

        UpdateCheckEnabled = _general.CreateEntry(UpdateCheckName, DefaultUpdateCheck,
            description: UpdateCheckPurpose);

        ColorBoard.ToggleKey = BindHotkey(BoardKeyName, DefaultBoardKey, BoardKeyPurpose);

        MelonPreferences.Save();
    }

    private KeyCode BindHotkey(string settingName, KeyCode defaultKey, string purpose)
    {
        MelonPreferences_Entry<string> entry = _hotkeys.CreateEntry(settingName, defaultKey.ToString(),
            description: HotkeyDescription(purpose));

        return ParseHotkey(settingName, entry.Value, defaultKey);
    }
}
#endif
