#if !MELONLOADER
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ReColor;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public partial class ReColorPlugin : BaseUnityPlugin
{
    private const string GeneralSection = "General";
    private const string HotkeysSection = "Hotkeys";
    private const string SectionSeparator = "# ----------------------------------------------------------------";

    internal static ConfigEntry<bool> UpdateCheckEnabled;

    private void Awake()
    {
        Log.Source = Logger;

        BindConfig();
        AddSectionSeparators();
        new Harmony(PluginGuid).PatchAll();

        RunSelfChecks();

        if (UpdateCheckEnabled.Value)
        {
            StartCoroutine(UpdateCheck.Run(PluginVersion));
        }
    }

    private void Update()
    {
        HandleHotkeys();
    }

    private void BindConfig()
    {
        UpdateCheckEnabled = Config.Bind(GeneralSection, UpdateCheckName, DefaultUpdateCheck,
            UpdateCheckPurpose);

        ColorBoard.ToggleKey = BindHotkey(BoardKeyName, DefaultBoardKey, BoardKeyPurpose);
    }

    private void AddSectionSeparators()
    {
        List<string> lines = new List<string>(File.ReadAllLines(Config.ConfigFilePath));
        bool hasChanged = false;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            bool isSectionHeader = lines[lineIndex].StartsWith("[");
            bool hasSeparatorAbove = lineIndex > 0 && lines[lineIndex - 1] == SectionSeparator;

            if (!isSectionHeader || hasSeparatorAbove)
                continue;

            lines.Insert(lineIndex, SectionSeparator);
            lineIndex++;
            hasChanged = true;
        }

        if (hasChanged)
        {
            File.WriteAllLines(Config.ConfigFilePath, lines);
        }
    }

    private KeyCode BindHotkey(string settingName, KeyCode defaultKey, string purpose)
    {
        ConfigEntry<string> entry = Config.Bind(HotkeysSection, settingName, defaultKey.ToString(),
            HotkeyDescription(purpose));

        return ParseHotkey(settingName, entry.Value, defaultKey);
    }
}
#endif
