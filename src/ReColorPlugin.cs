using System;
using UnityEngine;

namespace ReColor;

public partial class ReColorPlugin
{
    public const string PluginGuid = "com.archives.recolor";
    internal const string PluginName = "ReColor";
    internal const string PluginVersion = "1.0.0";
    internal const string PluginAuthor = "Archives";

    internal const string BoardKeyName = "BoardKey";
    internal const string UpdateCheckName = "UpdateCheck";

    internal const string BoardKeyPurpose = "Opens the recolour board while you are at the workbench.";

    internal const string UpdateCheckPurpose =
        "Looks up the latest release on github.com once at startup and logs a line when yours is older. "
        + "Sends nothing about you. Set to false to keep the mod entirely offline.";

    internal const KeyCode DefaultBoardKey = KeyCode.F11;
    internal const bool DefaultUpdateCheck = true;

    internal static string HotkeyDescription(string purpose)
    {
        return $"{purpose} Takes any Unity KeyCode name, e.g. F11, G, Tab, Keypad5. None turns it off.";
    }

    private static void RunSelfChecks()
    {
        bool hasPassedSelfCheck = UpdateCheck.SelfCheck();
        string outcome = hasPassedSelfCheck ? "Loaded. Self-check passed." : "Loaded. SELF-CHECK FAILED.";

        Log.Info(outcome);
    }

    private static void HandleHotkeys()
    {
        ColorBoard.Tick();
        SurfaceLook.Tick();
    }

    private static KeyCode ParseHotkey(string settingName, string configuredKey, KeyCode defaultKey)
    {
        bool isKnownKeyName = Enum.TryParse(configuredKey, true, out KeyCode key)
                              && Enum.IsDefined(typeof(KeyCode), key);

        if (isKnownKeyName)
            return key;

        Log.Warning($"{settingName} \"{configuredKey}\" is not a key name; falling back to {defaultKey}.");

        return defaultKey;
    }
}
