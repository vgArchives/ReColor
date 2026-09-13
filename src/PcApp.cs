using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Restory.Data.PC;
using Restory.Data.SaveLoad.Containers;
using Restory.Gameplay.PC;
using Restory.UI.Presenters;
using Restory.UI.Presenters.PC.Apps;
using Restory.UserInterface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReColor;

[HarmonyPatch]
internal static class PcApp
{
    private const string AppId = "com.archives.recolor.app";
    private const string AppLabel = "ReColor";
    private const int AppVersion = 1;

    private const string IconResource = "ReColor.ReColor_Icon.png";

    private const float IconScale = 0.85f;

    private static PcAppInfo _app;
    private static PcAppCategoryInfo _category;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PcAppManager), nameof(PcAppManager.Initialize))]
    private static void AddToDesktop(PcAppManager __instance)
    {
        TryAdd(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PcAppManager), nameof(PcAppManager.LaunchPcApp))]
    private static bool InterceptLaunch(PcAppInfo appInfo, GUI_PcWindowsXpScreen ___pcScreen)
    {
        if (!IsOurApp(appInfo))
            return true;

        if (___pcScreen != null)
        {
            ___pcScreen.Hide();
        }

        ColorBoard.Open();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GUI_PcAppIcon), nameof(GUI_PcAppIcon.Init))]
    private static void NameTheIcon(PcAppInfo appInfo, ref string appLocalizedName)
    {
        if (IsOurApp(appInfo))
        {
            appLocalizedName = AppLabel;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUI_PcAppIcon), nameof(GUI_PcAppIcon.Init))]
    private static void SizeTheIcon(PcAppInfo appInfo, Image ___image, TextMeshProUGUI ___appName)
    {
        if (!IsOurApp(appInfo) || ___image == null)
            return;

        ___image.rectTransform.localScale = Vector3.one * IconScale;

        if (___appName != null)
        {
            ___appName.rectTransform.localScale = Vector3.one / IconScale;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUI_PcAppIconsPanel), nameof(GUI_PcAppIconsPanel.CreateAppIcon))]
    private static void KeepIconLast(List<GUI_PcAppIcon> ___icons)
    {
        if (_app == null || ___icons == null)
            return;

        foreach (GUI_PcAppIcon icon in ___icons)
        {
            if (icon != null && icon.AppInfo == _app)
            {
                icon.transform.SetAsLastSibling();
                return;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUI_PcAppStartMenuButton), nameof(GUI_PcAppStartMenuButton.Init))]
    private static void NameTheStartMenuEntry(PcAppInfo appInfo, GUI_LocalisedText ___appName)
    {
        if (!IsOurApp(appInfo) || ___appName == null)
            return;

        ___appName.IsEnabled = false;

        TMP_Text label = ___appName.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.text = AppLabel;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GUI_PcWindowsXpStartMenu), nameof(GUI_PcWindowsXpStartMenu.CreateAppButton))]
    private static void KeepStartMenuEntryLast(List<GUI_PcAppStartMenuButton> ___appButtons)
    {
        if (_app == null || ___appButtons == null)
            return;

        foreach (GUI_PcAppStartMenuButton entry in ___appButtons)
        {
            if (entry != null && entry.AppInfo == _app)
            {
                entry.transform.SetAsLastSibling();
                return;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GUI_PcWindowsXpToolbar), nameof(GUI_PcWindowsXpToolbar.CreateAppButton))]
    private static bool SkipTaskbarButton(PcAppInfo appInfo)
    {
        return !IsOurApp(appInfo);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PcAppManager), nameof(PcAppManager.CaptureState))]
    private static void KeepOutOfSave(ref object __result)
    {
        if (_app == null)
            return;

        var saveData = __result as PcAppManagerSaveData;

        if (saveData == null)
            return;

        int appsBefore = Count(saveData.InstalledApps) + Count(saveData.AvailableApps);

        saveData.InstalledApps = WithoutTheApp(saveData.InstalledApps);
        saveData.AvailableApps = WithoutTheApp(saveData.AvailableApps);

        int appsAfter = Count(saveData.InstalledApps) + Count(saveData.AvailableApps);
        int droppedApps = appsBefore - appsAfter;

        if (droppedApps > 1)
        {
            Log.Warning($"ReColor app: {droppedApps} apps left the save where only the board's own was "
                        + "meant to. This should not be possible; the save may be missing an app.");
        }

        Log.Debug($"ReColor app: the save kept {appsAfter} of {appsBefore} app(s); the board's own app "
                  + "was left out.");
    }

    private static bool IsOurApp(PcAppInfo appInfo)
    {
        return _app != null && appInfo == _app;
    }

    private static int Count(List<PcAppInfo> apps)
    {
        return apps == null ? 0 : apps.Count;
    }

    private static List<PcAppInfo> WithoutTheApp(List<PcAppInfo> apps)
    {
        var kept = new List<PcAppInfo>();

        if (apps == null)
            return kept;

        foreach (PcAppInfo app in apps)
        {
            if (ReferenceEquals(app, _app))
                continue;

            kept.Add(app);
        }

        return kept;
    }

    private static void TryAdd(PcAppManager manager)
    {
        if (manager == null)
            return;

        if (_app == null)
        {
            Build();

            if (_app == null)
                return;
        }

        if (manager.ContainsApp(_app))
            return;

        manager.InstallPcApp(_app);

        Log.Info("ReColor app added to the desktop.");
    }

    private static void Build()
    {
        _category = ScriptableObject.CreateInstance<PcAppCategoryInfo>();
        _category.name = $"{AppLabel} - PcAppCategory";
        _category.hideFlags = HideFlags.HideAndDontSave;

        SetField(_category, "id", $"{AppId}.category");

        _app = ScriptableObject.CreateInstance<PcAppInfo>();
        _app.name = $"{AppLabel} - PcApp";
        _app.hideFlags = HideFlags.HideAndDontSave;

        Sprite icon = LoadIcon();

        (string Name, object Value)[] fields =
        {
            ("id", AppId),
            ("category", _category),
            ("version", AppVersion),
            ("nameLocalizationKey", AppLabel),
            ("desktopIcon", icon),
            ("guiLifecycleMode", PcAppGuiLifecycleMode.Destroy)
        };

        foreach ((string Name, object Value) field in fields)
        {
            SetField(_app, field.Name, field.Value);
        }

        Log.Info($"ReColor app icon: {(icon == null ? "none" : icon.name)}.");
    }

    private static Sprite LoadIcon()
    {
        Stream stream = typeof(PcApp).Assembly.GetManifestResourceStream(IconResource);

        if (stream == null)
        {
            Log.Warning($"ReColor app: the icon \"{IconResource}\" is missing from the build.");
            return null;
        }

        using (stream)
        using (var bytes = new MemoryStream())
        {
            stream.CopyTo(bytes);

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            if (!texture.LoadImage(bytes.ToArray()))
            {
                Log.Warning("ReColor app: the icon would not decode.");
                return null;
            }

            var bounds = new Rect(0f, 0f, texture.width, texture.height);
            Sprite icon = Sprite.Create(texture, bounds, new Vector2(0.5f, 0.5f));

            icon.name = $"{AppLabel} icon";
            icon.hideFlags = HideFlags.HideAndDontSave;

            return icon;
        }
    }

    private static void SetField(object target, string name, object value)
    {
        FieldInfo field = AccessTools.Field(target.GetType(), name);

        if (field == null)
        {
            Log.Warning($"ReColor app: the field \"{name}\" is gone, so the app may not show correctly.");
            return;
        }

        field.SetValue(target, value);
    }
}
