using System;
using System.Collections.Generic;
using HarmonyLib;
using Restory.Gameplay.Competitions;
using Restory.Gameplay.Workplace;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReColor;

[HarmonyPatch]
internal static class SurfaceLook
{
    private const string RugRendererField = "rugRenderer";
    private const string CompetitionRendererField = "competitionRugMeshRenderer";
    private const string WinEffectField = "competitionWinEffectObject";
    private const string MainModelField = "mainModel";
    private const string CompetitionModelField = "competitionModel";

    private const string TableMaterialName = "Environment_Table";
    internal const string TableSurfaceName = "Table";
    internal const string NormalRugSurfaceName = "Normal rug";
    internal const string TournamentSurfaceName = "Tournament rug";
    private const string BaseTextureProperty = "_Main_Texture";

    private const float PaintInterval = 0.05f;

    private const int PaintSliderCount = 2;
    private const float MaxPaintShaping = 3f;

    private const int ChannelCount = 3;

    private const int NoKnob = -1;

    private static readonly Color DefaultPaintColor = new Color(0.43f, 0.24f, 0.59f);

    private static readonly List<LookTarget> Targets = new List<LookTarget>();
    private static readonly int BaseTextureId = Shader.PropertyToID(BaseTextureProperty);

    private static WorkPlaceRugVisualizer _visualizer;
    private static WorkplaceRugSwitcher _switcher;
    private static CompetitionGameMode _competition;

    private static GameObject _mainModel;
    private static GameObject _competitionModel;
    private static GameObject _competitionTimer;
    private static GameObject _winEffect;

    private static bool _wasTimerActive;
    private static bool _wasWinEffectActive;

    private static float _nextPaintTime;

    private static RugMode _rugMode = RugMode.Normal;

    internal static int SurfaceCount => Targets.Count;

    private enum RugMode
    {
        Normal,
        Tournament
    }

    private enum BaseMode
    {
        Original,
        Greyscale,
        Flat
    }

    private sealed class FloatKnob
    {
        internal readonly string Label;
        internal readonly string Property;
        internal readonly int PropertyId;
        internal readonly float Min;
        internal readonly float Max;

        internal float Original;

        internal FloatKnob(string label, string property, float min, float max)
        {
            Label = label;
            Property = property;
            PropertyId = Shader.PropertyToID(property);
            Min = min;
            Max = max;
        }
    }

    private sealed class ColorKnob
    {
        internal readonly string Label;
        internal readonly string Property;
        internal readonly int PropertyId;

        internal Color Original;

        internal ColorKnob(string label, string property)
        {
            Label = label;
            Property = property;
            PropertyId = Shader.PropertyToID(property);
        }
    }

    private sealed class LookTarget
    {
        internal string Name;
        internal Material Material;
        internal BaseMode[] Modes;
        internal FloatKnob[] Floats;

        internal ColorKnob BaseColor;
        internal ColorKnob[] Colors;

        internal bool LinksColors;

        internal string[] ModeLabels;
        internal Texture OriginalBase;
        internal BaseMode Mode;
        internal int ModeIndex;

        internal byte[] Greyscale;
        internal Color32[] PaintBuffer;
        internal Texture2D Working;
        internal Texture2D Flat;

        internal Color Paint = DefaultPaintColor;
        internal float Brightness = 1f;
        internal float Contrast = 1f;
        internal bool IsDirty;

        internal bool HasOriginals;
    }

    internal static string SurfaceName(int surfaceIndex)
    {
        return IsSurface(surfaceIndex) ? Targets[surfaceIndex].Name : "No surface";
    }

    internal static void PaintSurface(int surfaceIndex, Color color)
    {
        if (!IsSurface(surfaceIndex))
            return;

        EnsureOriginals(surfaceIndex);

        LookTarget target = Targets[surfaceIndex];

        if (target.BaseColor != null)
        {
            target.Material.SetColor(target.BaseColor.PropertyId, color);
            return;
        }

        target.Paint = color;

        if (target.Mode == BaseMode.Flat)
        {
            FlatTextureFor(target);
            return;
        }

        target.IsDirty = true;
    }

    internal static Color SurfaceColor(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return Color.white;

        LookTarget target = Targets[surfaceIndex];

        if (target.BaseColor != null)
            return target.Material.GetColor(target.BaseColor.PropertyId);

        return target.Paint;
    }

    internal static void ShowSurface(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return;

        string name = Targets[surfaceIndex].Name;

        if (name == TournamentSurfaceName)
        {
            _rugMode = RugMode.Tournament;
        }
        else if (name == NormalRugSurfaceName)
        {
            _rugMode = RugMode.Normal;
        }
        else
        {
            return;
        }

        ApplyRugMode();
    }

    internal static string[] ModeLabels(int surfaceIndex)
    {
        return IsSurface(surfaceIndex) ? Targets[surfaceIndex].ModeLabels : new string[0];
    }

    internal static int ModeIndexOf(int surfaceIndex)
    {
        return IsSurface(surfaceIndex) ? Targets[surfaceIndex].ModeIndex : 0;
    }

    internal static void SetMode(int surfaceIndex, int modeIndex)
    {
        if (!IsSurface(surfaceIndex))
            return;

        EnsureOriginals(surfaceIndex);

        LookTarget target = Targets[surfaceIndex];

        if (modeIndex < 0 || modeIndex >= target.Modes.Length)
            return;

        target.ModeIndex = modeIndex;
        target.Mode = target.Modes[modeIndex];

        ApplyBase(target);
    }

    internal static int ColorTargetCount(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return 0;

        LookTarget target = Targets[surfaceIndex];

        return target.LinksColors ? 1 : 1 + target.Colors.Length;
    }

    internal static string ColorTargetName(int surfaceIndex, int targetIndex)
    {
        if (!IsSurface(surfaceIndex))
            return string.Empty;

        LookTarget target = Targets[surfaceIndex];

        if (targetIndex <= 0)
        {
            if (target.LinksColors)
                return "Colour";

            return target.BaseColor != null ? target.BaseColor.Label : "Base paint";
        }

        return targetIndex <= target.Colors.Length ? target.Colors[targetIndex - 1].Label : string.Empty;
    }

    internal static Color ColorTarget(int surfaceIndex, int targetIndex)
    {
        if (!IsSurface(surfaceIndex))
            return Color.white;

        LookTarget target = Targets[surfaceIndex];

        if (targetIndex <= 0)
            return SurfaceColor(surfaceIndex);

        if (targetIndex > target.Colors.Length)
            return Color.white;

        return target.Material.GetColor(target.Colors[targetIndex - 1].PropertyId);
    }

    internal static void SetColorTarget(int surfaceIndex, int targetIndex, Color color)
    {
        if (!IsSurface(surfaceIndex))
            return;

        EnsureOriginals(surfaceIndex);

        LookTarget target = Targets[surfaceIndex];

        if (target.LinksColors)
        {
            PaintSurface(surfaceIndex, color);

            foreach (ColorKnob knob in target.Colors)
            {
                target.Material.SetColor(knob.PropertyId, color);
            }

            return;
        }

        if (targetIndex <= 0)
        {
            PaintSurface(surfaceIndex, color);
            return;
        }

        if (targetIndex <= target.Colors.Length)
        {
            target.Material.SetColor(target.Colors[targetIndex - 1].PropertyId, color);
        }
    }

    internal static int SliderCount(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return 0;

        LookTarget target = Targets[surfaceIndex];

        return target.Floats.Length + (target.BaseColor == null ? PaintSliderCount : 0);
    }

    internal static string SliderLabel(int surfaceIndex, int sliderIndex)
    {
        if (!TryReadSlider(surfaceIndex, sliderIndex, out LookTarget target, out int knobIndex))
            return string.Empty;

        if (knobIndex != NoKnob)
            return target.Floats[knobIndex].Label;

        return sliderIndex == 0 ? "Brightness" : "Contrast";
    }

    internal static Vector2 SliderRange(int surfaceIndex, int sliderIndex)
    {
        if (!TryReadSlider(surfaceIndex, sliderIndex, out LookTarget target, out int knobIndex))
            return new Vector2(0f, 1f);

        if (knobIndex != NoKnob)
            return new Vector2(target.Floats[knobIndex].Min, target.Floats[knobIndex].Max);

        return new Vector2(0f, MaxPaintShaping);
    }

    internal static float SliderValue(int surfaceIndex, int sliderIndex)
    {
        if (!TryReadSlider(surfaceIndex, sliderIndex, out LookTarget target, out int knobIndex))
            return 0f;

        if (knobIndex != NoKnob)
            return target.Material.GetFloat(target.Floats[knobIndex].PropertyId);

        return sliderIndex == 0 ? target.Brightness : target.Contrast;
    }

    internal static void SetSliderValue(int surfaceIndex, int sliderIndex, float value)
    {
        if (!TryReadSlider(surfaceIndex, sliderIndex, out LookTarget target, out int knobIndex))
            return;

        EnsureOriginals(surfaceIndex);

        if (knobIndex != NoKnob)
        {
            target.Material.SetFloat(target.Floats[knobIndex].PropertyId, value);
            return;
        }

        if (sliderIndex == 0)
        {
            target.Brightness = value;
        }
        else
        {
            target.Contrast = value;
        }

        RepaintFromPaintValues(target);
    }

    internal static void ResetSurface(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return;

        EnsureOriginals(surfaceIndex);

        LookTarget target = Targets[surfaceIndex];

        if (target.BaseColor != null)
        {
            target.Material.SetColor(target.BaseColor.PropertyId, target.BaseColor.Original);
        }

        foreach (ColorKnob knob in target.Colors)
        {
            target.Material.SetColor(knob.PropertyId, knob.Original);
        }

        foreach (FloatKnob knob in target.Floats)
        {
            target.Material.SetFloat(knob.PropertyId, knob.Original);
        }

        target.Mode = BaseMode.Original;
        target.ModeIndex = 0;
        target.Paint = DefaultPaintColor;
        target.Brightness = 1f;
        target.Contrast = 1f;

        ApplyBase(target);

        Log.Info($"ReColor: \"{target.Name}\" reset to the game's own values.");
    }

    internal static float[] CaptureState(int surfaceIndex)
    {
        int colorCount = ColorTargetCount(surfaceIndex);
        int sliderCount = SliderCount(surfaceIndex);
        var state = new float[1 + colorCount * ChannelCount + sliderCount];

        state[0] = ModeIndexOf(surfaceIndex);

        for (int target = 0; target < colorCount; target++)
        {
            Color color = ColorTarget(surfaceIndex, target);

            state[1 + target * ChannelCount] = color.r;
            state[2 + target * ChannelCount] = color.g;
            state[3 + target * ChannelCount] = color.b;
        }

        for (int slider = 0; slider < sliderCount; slider++)
        {
            state[1 + colorCount * ChannelCount + slider] = SliderValue(surfaceIndex, slider);
        }

        return state;
    }

    internal static void ApplyState(int surfaceIndex, float[] state)
    {
        int colorCount = ColorTargetCount(surfaceIndex);
        int sliderCount = SliderCount(surfaceIndex);

        bool isWrongLength = state == null || state.Length != 1 + colorCount * ChannelCount + sliderCount;

        if (isWrongLength)
        {
            Log.Warning("ReColor: a preset does not fit this surface; it was not applied.");
            return;
        }

        SetMode(surfaceIndex, Mathf.RoundToInt(state[0]));

        for (int target = 0; target < colorCount; target++)
        {
            float red = state[1 + target * ChannelCount];
            float green = state[2 + target * ChannelCount];
            float blue = state[3 + target * ChannelCount];

            SetColorTarget(surfaceIndex, target, new Color(red, green, blue, 1f));
        }

        for (int slider = 0; slider < sliderCount; slider++)
        {
            SetSliderValue(surfaceIndex, slider, state[1 + colorCount * ChannelCount + slider]);
        }
    }

    internal static void Tick()
    {
        RepaintDirtyTargets();
    }

    private static bool TryReadSlider(int surfaceIndex, int sliderIndex, out LookTarget target, out int knobIndex)
    {
        target = null;
        knobIndex = NoKnob;

        if (!IsSurface(surfaceIndex) || sliderIndex < 0)
            return false;

        target = Targets[surfaceIndex];

        bool hasPaintSliders = target.BaseColor == null;

        if (hasPaintSliders && sliderIndex < PaintSliderCount)
            return true;

        knobIndex = hasPaintSliders ? sliderIndex - PaintSliderCount : sliderIndex;

        return knobIndex < target.Floats.Length;
    }

    private static void RepaintFromPaintValues(LookTarget target)
    {
        if (target.Mode == BaseMode.Flat)
        {
            FlatTextureFor(target);
            return;
        }

        target.IsDirty = true;
    }

    private static bool IsSurface(int surfaceIndex)
    {
        return surfaceIndex >= 0 && surfaceIndex < Targets.Count;
    }

    private static void EnsureOriginals(int surfaceIndex)
    {
        if (!IsSurface(surfaceIndex))
            return;

        LookTarget target = Targets[surfaceIndex];

        if (target.HasOriginals || target.Material == null)
            return;

        target.HasOriginals = true;

        if (target.BaseColor != null)
        {
            target.BaseColor.Original = target.Material.GetColor(target.BaseColor.PropertyId);
        }

        foreach (ColorKnob knob in target.Colors)
        {
            knob.Original = target.Material.GetColor(knob.PropertyId);
        }

        foreach (FloatKnob knob in target.Floats)
        {
            knob.Original = target.Material.GetFloat(knob.PropertyId);
        }

        target.OriginalBase = target.Material.GetTexture(BaseTextureId);

        Log.Debug($"ReColor: \"{target.Name}\" originals taken at first change.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkSurface), nameof(WorkSurface.Initialize))]
    private static void BindToBench(WorkSurface __instance)
    {
        Unbind();

        _visualizer = __instance.RugVisualizer;

        if (_visualizer == null)
        {
            Log.Warning("ReColor: the work surface has no rug visualizer; panel disabled.");
            return;
        }

        _switcher = _visualizer.GetComponent<WorkplaceRugSwitcher>();
        _competition = Object.FindAnyObjectByType<CompetitionGameMode>();

        if (!ReadRugObjects(out Renderer rugRenderer, out Renderer competitionRenderer))
            return;

        _wasTimerActive = _competitionTimer != null && _competitionTimer.activeSelf;
        _wasWinEffectActive = _winEffect != null && _winEffect.activeSelf;

        Register(new LookTarget
        {
            Name = TableSurfaceName,
            Material = FindSceneMaterial(TableMaterialName),
            Modes = new[] { BaseMode.Original, BaseMode.Flat },
            BaseColor = new ColorKnob("Colour", "_Color"),
            Colors = new ColorKnob[0],
            Floats = TableFloats()
        });

        Register(new LookTarget
        {
            Name = NormalRugSurfaceName,
            Material = InstanceOf(rugRenderer, 0),
            Modes = new[] { BaseMode.Original, BaseMode.Greyscale, BaseMode.Flat },
            BaseColor = new ColorKnob("Colour", "_Color"),
            Colors = new ColorKnob[0],
            Floats = RugFloats()
        });

        Register(new LookTarget
        {
            Name = TournamentSurfaceName,
            Material = InstanceOf(competitionRenderer, 0),
            Modes = new[] { BaseMode.Original, BaseMode.Greyscale, BaseMode.Flat },
            Colors = TournamentColors(),
            LinksColors = true,
            Floats = TournamentFloats()
        });

        ApplyRugMode();

        Log.Debug($"ReColor bound to {Targets.Count} material(s).");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkSurface), nameof(WorkSurface.Dispose))]
    private static void UnbindFromBench()
    {
        Unbind();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkplaceRugSwitcher), nameof(WorkplaceRugSwitcher.SwitchRug))]
    private static void ReapplyAfterGameSwitch()
    {
        ApplyRugMode();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorkPlaceRugVisualizer), nameof(WorkPlaceRugVisualizer.PlayCompetitionWinEffect))]
    private static void ReapplyAfterWinEffect()
    {
        ApplyRugMode();
    }

    private static bool ReadRugObjects(out Renderer rugRenderer, out Renderer competitionRenderer)
    {
        rugRenderer = null;
        competitionRenderer = null;

        Type visualizerType = typeof(WorkPlaceRugVisualizer);
        Type switcherType = typeof(WorkplaceRugSwitcher);

        bool hasEveryVisualizerField = AccessTools.Field(visualizerType, RugRendererField) != null
                                       && AccessTools.Field(visualizerType, CompetitionRendererField) != null
                                       && AccessTools.Field(visualizerType, WinEffectField) != null;

        bool hasEverySwitcherField = _switcher != null
                                     && AccessTools.Field(switcherType, MainModelField) != null
                                     && AccessTools.Field(switcherType, CompetitionModelField) != null;

        if (!hasEveryVisualizerField || !hasEverySwitcherField)
        {
            Log.Warning("ReColor: the rug visualizer or switcher is missing a field, most "
                        + "likely renamed by a game update. The panel stays closed; nothing else is affected.");

            return false;
        }

        rugRenderer = AccessTools.Field(visualizerType, RugRendererField).GetValue(_visualizer) as Renderer;

        competitionRenderer = AccessTools.Field(visualizerType, CompetitionRendererField)
            .GetValue(_visualizer) as Renderer;

        _winEffect = AccessTools.Field(visualizerType, WinEffectField).GetValue(_visualizer) as GameObject;

        _mainModel = AccessTools.Field(switcherType, MainModelField).GetValue(_switcher) as GameObject;

        _competitionModel = AccessTools.Field(switcherType, CompetitionModelField).GetValue(_switcher) as GameObject;

        if (_competitionModel != null)
        {
            Canvas timerCanvas = _competitionModel.GetComponentInChildren<Canvas>(true);
            _competitionTimer = timerCanvas == null ? null : timerCanvas.gameObject;
        }

        return rugRenderer != null && competitionRenderer != null;
    }

    private static void Register(LookTarget target)
    {
        if (target.Material == null)
        {
            Log.Warning($"ReColor: material for \"{target.Name}\" not found; that section is skipped.");
            return;
        }

        if (target.BaseColor != null)
        {
            target.BaseColor.Original = target.Material.GetColor(target.BaseColor.PropertyId);
        }

        foreach (ColorKnob knob in target.Colors)
        {
            knob.Original = target.Material.GetColor(knob.PropertyId);
        }

        foreach (FloatKnob knob in target.Floats)
        {
            knob.Original = target.Material.GetFloat(knob.PropertyId);
        }

        target.ModeLabels = new string[target.Modes.Length];

        for (int index = 0; index < target.Modes.Length; index++)
        {
            target.ModeLabels[index] = target.Modes[index].ToString();
        }

        target.OriginalBase = target.Material.GetTexture(BaseTextureId);

        Targets.Add(target);
    }

    private static void Unbind()
    {
        foreach (LookTarget target in Targets)
        {
            DestroyTexture(ref target.Working);
            DestroyTexture(ref target.Flat);
        }

        Targets.Clear();

        _visualizer = null;
        _switcher = null;
        _competition = null;
        _mainModel = null;
        _competitionModel = null;
        _competitionTimer = null;
        _winEffect = null;
    }

    private static void DestroyTexture(ref Texture2D texture)
    {
        if (texture != null)
        {
            Object.Destroy(texture);
            texture = null;
        }
    }

    private static bool IsCompetitionRunning()
    {
        return _competition != null && _competition.HasDeviceInCompetition;
    }

    private static void ApplyRugMode()
    {
        if (_mainModel == null || _competitionModel == null)
            return;

        if (IsCompetitionRunning())
        {
            SetActive(_competitionTimer, _wasTimerActive);
            return;
        }

        bool wantsTournament = _rugMode == RugMode.Tournament;

        SetActive(_mainModel, !wantsTournament);
        SetActive(_competitionModel, wantsTournament);

        if (!wantsTournament)
        {
            SetActive(_competitionTimer, _wasTimerActive);
            SetActive(_winEffect, _wasWinEffectActive);
            return;
        }

        SetActive(_competitionTimer, false);
        SetActive(_winEffect, false);
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null && target.activeSelf != isActive)
        {
            target.SetActive(isActive);
        }
    }

    private static Material InstanceOf(Renderer renderer, int slot)
    {
        if (renderer == null)
            return null;

        Material[] instances = renderer.materials;

        return slot < instances.Length ? instances[slot] : null;
    }

    private static Material FindSceneMaterial(string materialName)
    {
        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            Material[] shared = renderer.sharedMaterials;

            for (int slot = 0; slot < shared.Length; slot++)
            {
                if (shared[slot] != null && shared[slot].name.StartsWith(materialName))
                    return InstanceOf(renderer, slot);
            }
        }

        return null;
    }

    private static FloatKnob[] TableFloats()
    {
        return new[]
        {
            new FloatKnob("Emission power", "_Emission_Power", 0f, 10f),
            new FloatKnob("Specular", "_Specular", 0f, 1f),
            new FloatKnob("Metallic", "_Metallic", 0f, 1f)
        };
    }

    private static FloatKnob[] RugFloats()
    {
        return new[]
        {
            new FloatKnob("Emission power", "_Emission_Power", 0f, 10f)
        };
    }

    private static ColorKnob[] TournamentColors()
    {
        return new[]
        {
            new ColorKnob("Stripes", "_Color_Stripes"),
            new ColorKnob("Stripes additive", "_Color_Stripes_Addective"),
            new ColorKnob("Shine", "_Color_Shine")
        };
    }

    private static FloatKnob[] TournamentFloats()
    {
        return new[]
        {
            new FloatKnob("Shine intensity", "_Shine_Intensity", 0f, 4f)
        };
    }

    private static void ApplyBase(LookTarget target)
    {
        if (target.Mode == BaseMode.Original)
        {
            target.Material.SetTexture(BaseTextureId, target.OriginalBase);
            return;
        }

        if (target.Mode == BaseMode.Flat)
        {
            target.Material.SetTexture(BaseTextureId, FlatTextureFor(target));
            return;
        }

        if (!TryBuildGreyscale(target))
            return;

        target.Material.SetTexture(BaseTextureId, target.Working);

        if (target.BaseColor == null)
        {
            target.IsDirty = true;
        }
    }

    private static Texture2D FlatTextureFor(LookTarget target)
    {
        if (target.Flat == null)
        {
            target.Flat = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        }

        Color color = target.BaseColor != null ? Color.white : target.Paint * target.Brightness;

        target.Flat.SetPixel(0, 0, color);
        target.Flat.Apply(false);

        return target.Flat;
    }

    private static bool TryBuildGreyscale(LookTarget target)
    {
        if (target.Greyscale != null)
            return true;

        if (target.OriginalBase == null)
        {
            Log.Warning($"ReColor: \"{target.Name}\" has no base texture to greyscale.");
            return false;
        }

        int width = target.OriginalBase.width;
        int height = target.OriginalBase.height;

        RenderTexture buffer = RenderTexture.GetTemporary(width, height, 0,
            RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        RenderTexture previous = RenderTexture.active;

        Graphics.Blit(target.OriginalBase, buffer);
        RenderTexture.active = buffer;

        target.Working = new Texture2D(width, height, TextureFormat.RGBA32, false);
        target.Working.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        target.Working.Apply(false);

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(buffer);

        BuildLuminance(target, target.Working.GetPixels32());
        WriteGreyscale(target);

        Log.Debug($"ReColor built a {width}x{height} greyscale base for \"{target.Name}\".");

        return true;
    }

    private static void BuildLuminance(LookTarget target, Color32[] source)
    {
        target.Greyscale = new byte[source.Length];
        target.PaintBuffer = new Color32[source.Length];

        byte darkest = byte.MaxValue;
        byte lightest = byte.MinValue;

        for (int index = 0; index < source.Length; index++)
        {
            Color32 pixel = source[index];
            var luminance = (byte)((pixel.r * 77 + pixel.g * 150 + pixel.b * 29) >> 8);

            target.Greyscale[index] = luminance;

            if (luminance < darkest)
            {
                darkest = luminance;
            }

            if (luminance > lightest)
            {
                lightest = luminance;
            }
        }

        if (lightest <= darkest)
            return;

        float spread = 255f / (lightest - darkest);

        for (int index = 0; index < target.Greyscale.Length; index++)
        {
            target.Greyscale[index] = (byte)Mathf.Clamp(
                Mathf.RoundToInt((target.Greyscale[index] - darkest) * spread), 0, 255);
        }
    }

    private static void WriteGreyscale(LookTarget target)
    {
        for (int index = 0; index < target.Greyscale.Length; index++)
        {
            byte grey = target.Greyscale[index];
            target.PaintBuffer[index] = new Color32(grey, grey, grey, byte.MaxValue);
        }

        target.Working.SetPixels32(target.PaintBuffer);
        target.Working.Apply(false);
    }

    private static void RepaintDirtyTargets()
    {
        if (Time.unscaledTime < _nextPaintTime)
            return;

        bool hasRepainted = false;

        foreach (LookTarget target in Targets)
        {
            if (!target.IsDirty || target.Greyscale == null)
                continue;

            target.IsDirty = false;
            RepaintTarget(target);
            hasRepainted = true;
        }

        if (hasRepainted)
        {
            _nextPaintTime = Time.unscaledTime + PaintInterval;
        }
    }

    private static void RepaintTarget(LookTarget target)
    {
        for (int index = 0; index < target.Greyscale.Length; index++)
        {
            float grey = target.Greyscale[index] / 255f;

            grey = ((grey - 0.5f) * target.Contrast + 0.5f) * target.Brightness;

            target.PaintBuffer[index] = new Color32(
                ToByte(grey * target.Paint.r),
                ToByte(grey * target.Paint.g),
                ToByte(grey * target.Paint.b),
                byte.MaxValue);
        }

        target.Working.SetPixels32(target.PaintBuffer);
        target.Working.Apply(false);
    }

    private static byte ToByte(float value)
    {
        return (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255f), 0, 255);
    }
}
