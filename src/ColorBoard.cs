using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ReColor;

internal static class ColorBoard
{
    internal static KeyCode ToggleKey = KeyCode.F11;

    private const int Columns = 2;

    private const int PresetColumns = 5;

    private const float ChannelScale = 255f;

    private const int ChannelCount = 3;

    private const int NoSurface = -1;

    private static GameObject _board;
    private static GameObject _settingsPage;
    private static GameObject _presetsPage;

    private static DropdownRow _surfaceDropdown;
    private static DropdownRow _modeDropdown;
    private static DropdownRow _targetDropdown;
    private static DropdownRow _presetDropdown;

    private static readonly List<SliderRow> Rows = new List<SliderRow>();
    private static readonly List<LookPreset> Presets = new List<LookPreset>();

    private static int _surface;
    private static int _presetSurface;
    private static int _colorTarget;
    private static bool _isSyncing;

    private static TMP_Text _captionLook;

    private static readonly Color[] ChannelColors =
    {
        new Color(1f, 0.45f, 0.40f),
        new Color(0.42f, 0.76f, 0.32f),
        new Color(0.36f, 0.62f, 0.95f)
    };

    private static readonly string[] PreferredOrder =
    {
        SurfaceLook.NormalRugSurfaceName,
        SurfaceLook.TournamentSurfaceName,
        SurfaceLook.TableSurfaceName
    };

    private static readonly List<int> SurfaceOrder = new List<int>();

    private sealed class LookPreset
    {
        internal int Surface;
        internal string Name;
        internal Color Swatch;
        internal float[] State;
    }

    internal static void Tick()
    {
        if (Keys.WasPressed(ToggleKey))
        {
            Toggle();
        }
    }

    internal static void Toggle()
    {
        if (_board == null && !TryBuild())
            return;

        bool shouldOpen = !_board.activeSelf;

        if (shouldOpen)
        {
            ClampSurface();
            RebuildForSurface();
        }

        _board.SetActive(shouldOpen);
        Log.Debug($"ReColor board {(shouldOpen ? "open" : "closed")}.");
    }

    internal static void Close()
    {
        if (_board != null && _board.activeSelf)
        {
            _board.SetActive(false);
        }
    }

    private static bool TryBuild()
    {
        if (!BoardParts.TryClonePanel(out BoardParts.Panel panel))
            return false;

        _board = panel.Root;
        _settingsPage = panel.SettingsPage;

        BuildSurfaceOrder();

        _surface = SurfaceOrder.Count > 0 ? SurfaceOrder[0] : 0;

        BuildDropdowns(panel);
        BuildSliderRows(panel);
        BuildPresetPage(panel);
        BuildTabs(panel);

        ShowPresets(false);

        Log.Debug($"ReColor board built: {Rows.Count} slider row(s), {Presets.Count} preset(s).");

        return true;
    }

    private static void BuildDropdowns(BoardParts.Panel panel)
    {
        _surfaceDropdown = panel.CloneDropdownSlot("Surface", 0, 2);
        _modeDropdown = panel.CloneDropdownSlot("ColourMode", 1, 2);
        _targetDropdown = panel.CloneDropdownSlot("ColourTarget", 2, 3);

        if (_surfaceDropdown != null)
        {
            _surfaceDropdown.SetCaption("Surface style");
            _surfaceDropdown.SetOptions(SurfaceNames());
            _surfaceDropdown.OnChanged = ResolveSurfaceChosen;
        }

        if (_modeDropdown != null)
        {
            _modeDropdown.SetCaption("Base texture");
            _modeDropdown.OnChanged = ResolveModeChosen;
        }

        if (_targetDropdown != null)
        {
            _targetDropdown.SetCaption("Colour");
            _targetDropdown.OnChanged = ResolveTargetChosen;
        }

        _captionLook = _surfaceDropdown?.BarText;

        _surfaceDropdown?.SetCaptionLook(_captionLook);
        _modeDropdown?.SetCaptionLook(_captionLook);
        _targetDropdown?.SetCaptionLook(_captionLook);
    }

    private static void BuildSliderRows(BoardParts.Panel panel)
    {
        Rows.Clear();

        for (int index = 0; index < BoardParts.RowsPerColumn * Columns; index++)
        {
            SliderRow row = panel.CloneSliderRow($"Row{index}");

            if (row == null)
                continue;

            int captured = Rows.Count;
            row.OnChanged = value => ResolveRowChanged(captured, value);

            Rows.Add(row);
        }
    }

    private static void LayOutRows(int rowCount)
    {
        int perColumn = BoardParts.RowsPerColumn;
        float top = BoardParts.RowsTop(Mathf.Min(rowCount, perColumn));
        float pitch = BoardParts.RowPitch();

        for (int index = 0; index < Rows.Count; index++)
        {
            bool isUsed = index < rowCount;

            Rows[index].SetVisible(isUsed);

            if (!isUsed)
                continue;

            int column = index / perColumn;
            int slot = index % perColumn;

            Rows[index].Place(BoardParts.SlotCentre(column, Columns), top - pitch * slot);
        }
    }

    private static void BuildPresetPage(BoardParts.Panel panel)
    {
        _presetsPage = panel.BuildPresetGrid(PresetColumns);

        if (_presetsPage == null)
            return;

        BuildPresets();

        _presetDropdown = panel.CloneDropdownSlot("PresetSurface", _presetsPage.transform, 0, 2);

        if (_presetDropdown != null)
        {
            _presetDropdown.SetCaption("Surface style");
            _presetDropdown.SetCaptionLook(_captionLook);
            _presetDropdown.SetOptions(SurfaceNames());
            _presetDropdown.OnChanged = ResolvePresetSurfaceChosen;
        }

        panel.FillPresetGrid(Presets.Count, ResolvePresetClicked, PresetSwatch, PresetLabel,
            PresetIsVisible);
    }

    private static void BuildPresets()
    {
        Presets.Clear();

        for (int surface = 0; surface < SurfaceLook.SurfaceCount; surface++)
        {
            float[] template = SurfaceLook.CaptureState(surface);
            int sliderStart = 1 + SurfaceLook.ColorTargetCount(surface) * ChannelCount;

            foreach (BoardPresets.Preset preset in BoardPresets.For(SurfaceLook.SurfaceName(surface)))
            {
                Color swatch = BoardPresets.ColorOf(preset);
                var state = (float[])template.Clone();

                state[0] = preset.Mode;
                state[1] = swatch.r;
                state[2] = swatch.g;
                state[3] = swatch.b;

                int sliderCount = Mathf.Min(preset.Sliders.Length, state.Length - sliderStart);

                for (int slider = 0; slider < sliderCount; slider++)
                {
                    state[sliderStart + slider] = preset.Sliders[slider];
                }

                Presets.Add(new LookPreset
                {
                    Surface = surface,
                    Name = preset.Name,
                    Swatch = swatch,
                    State = state
                });
            }
        }
    }

    private static void BuildTabs(BoardParts.Panel panel)
    {
        panel.WireTab(BoardParts.TabSlot.Settings, () => ShowPresets(false));
        panel.WireTab(BoardParts.TabSlot.Presets, () => ShowPresets(true));
        panel.WireTab(BoardParts.TabSlot.Reset, ResolveResetClicked);
        panel.WireClose(Close);
    }

    private static void BuildSurfaceOrder()
    {
        SurfaceOrder.Clear();

        foreach (string wanted in PreferredOrder)
        {
            int surface = SurfaceNamed(wanted);

            if (surface != NoSurface)
            {
                SurfaceOrder.Add(surface);
            }
        }

        for (int surface = 0; surface < SurfaceLook.SurfaceCount; surface++)
        {
            if (!SurfaceOrder.Contains(surface))
            {
                SurfaceOrder.Add(surface);
            }
        }
    }

    private static int SurfaceNamed(string name)
    {
        for (int surface = 0; surface < SurfaceLook.SurfaceCount; surface++)
        {
            if (SurfaceLook.SurfaceName(surface) == name)
                return surface;
        }

        return NoSurface;
    }

    private static string[] SurfaceNames()
    {
        var names = new string[SurfaceOrder.Count];

        for (int slot = 0; slot < names.Length; slot++)
        {
            names[slot] = ShortSurfaceName(SurfaceLook.SurfaceName(SurfaceOrder[slot]));
        }

        return names;
    }

    private static string ShortSurfaceName(string surfaceName)
    {
        if (surfaceName == SurfaceLook.NormalRugSurfaceName)
            return "Normal";

        return surfaceName == SurfaceLook.TournamentSurfaceName ? "Tournament" : surfaceName;
    }

    private static void ShowPresets(bool wantsPresets)
    {
        if (wantsPresets)
        {
            _presetSurface = _surface;
            _presetDropdown?.SetValue(Mathf.Max(0, SurfaceOrder.IndexOf(_presetSurface)));

            BoardParts.FillPresetVisibility(PresetIsVisible);
        }

        if (_settingsPage != null)
        {
            _settingsPage.SetActive(!wantsPresets);
        }

        if (_presetsPage != null)
        {
            _presetsPage.SetActive(wantsPresets);
        }

        BoardParts.TabSlot openTab = wantsPresets ? BoardParts.TabSlot.Presets : BoardParts.TabSlot.Settings;

        BoardParts.HighlightTab(openTab);
    }

    private static bool PresetIsVisible(int presetIndex)
    {
        return presetIndex < Presets.Count && Presets[presetIndex].Surface == _presetSurface;
    }

    private static Color PresetSwatch(int presetIndex)
    {
        return presetIndex < Presets.Count ? Presets[presetIndex].Swatch : Color.white;
    }

    private static string PresetLabel(int presetIndex)
    {
        if (presetIndex >= Presets.Count)
            return string.Empty;

        int slot = 0;

        for (int index = 0; index <= presetIndex; index++)
        {
            if (Presets[index].Surface == Presets[presetIndex].Surface)
            {
                slot++;
            }
        }

        return slot.ToString();
    }

    private static void ResolvePresetClicked(int presetIndex)
    {
        if (presetIndex >= Presets.Count)
            return;

        LookPreset preset = Presets[presetIndex];

        _surface = preset.Surface;
        _colorTarget = 0;

        SurfaceLook.ShowSurface(_surface);
        SurfaceLook.ApplyState(_surface, preset.State);

        RebuildForSurface();
    }

    private static void ResolvePresetSurfaceChosen(int slot)
    {
        bool isOutsideSurfaceOrder = slot < 0 || slot >= SurfaceOrder.Count;

        if (_isSyncing || isOutsideSurfaceOrder)
            return;

        _presetSurface = SurfaceOrder[slot];

        BoardParts.FillPresetVisibility(PresetIsVisible);
    }

    private static void ResolveResetClicked()
    {
        SurfaceLook.ResetSurface(_surface);
        RebuildForSurface();
    }

    private static void ResolveSurfaceChosen(int slot)
    {
        bool isOutsideSurfaceOrder = slot < 0 || slot >= SurfaceOrder.Count;

        if (_isSyncing || isOutsideSurfaceOrder)
            return;

        _surface = SurfaceOrder[slot];
        _colorTarget = 0;

        SurfaceLook.ShowSurface(_surface);
        RebuildForSurface();
    }

    private static void ResolveModeChosen(int modeIndex)
    {
        if (_isSyncing)
            return;

        SurfaceLook.SetMode(_surface, modeIndex);
        SyncControls();
    }

    private static void ResolveTargetChosen(int targetIndex)
    {
        if (_isSyncing)
            return;

        _colorTarget = targetIndex;
        SyncControls();
    }

    private static void ResolveRowChanged(int rowIndex, float value)
    {
        if (_isSyncing)
            return;

        if (rowIndex < ChannelCount)
        {
            Color color = SurfaceLook.ColorTarget(_surface, _colorTarget);
            color[rowIndex] = value / ChannelScale;

            SurfaceLook.SetColorTarget(_surface, _colorTarget, color);
            return;
        }

        SurfaceLook.SetSliderValue(_surface, rowIndex - ChannelCount, value);
    }

    private static void RebuildForSurface()
    {
        _isSyncing = true;

        if (_modeDropdown != null)
        {
            _modeDropdown.SetOptions(SurfaceLook.ModeLabels(_surface));
        }

        int targetCount = SurfaceLook.ColorTargetCount(_surface);
        bool needsTargets = targetCount > 1;

        if (_targetDropdown != null)
        {
            _targetDropdown.SetVisible(needsTargets);

            if (needsTargets)
            {
                var names = new string[targetCount];

                for (int target = 0; target < targetCount; target++)
                {
                    names[target] = SurfaceLook.ColorTargetName(_surface, target);
                }

                _targetDropdown.SetOptions(names);
            }
        }

        if (_colorTarget >= targetCount)
        {
            _colorTarget = 0;
        }

        LayOutTopBand(needsTargets);

        int rowCount = Mathf.Min(Rows.Count, ChannelCount + SurfaceLook.SliderCount(_surface));

        LayOutRows(rowCount);

        for (int index = 0; index < rowCount; index++)
        {
            if (index < ChannelCount)
            {
                Rows[index].SetLabel(ChannelLabel(index));
                Rows[index].SetLabelColor(ChannelColors[index]);
                Rows[index].SetRange(0f, ChannelScale, true);
                continue;
            }

            int sliderIndex = index - ChannelCount;
            Vector2 range = SurfaceLook.SliderRange(_surface, sliderIndex);

            Rows[index].SetLabel(SurfaceLook.SliderLabel(_surface, sliderIndex));
            Rows[index].SetLabelLook(_captionLook);
            Rows[index].SetRange(range.x, range.y, false);
        }

        _isSyncing = false;

        SyncControls();
        BoardParts.FillPresetVisibility(PresetIsVisible);
    }

    private static void LayOutTopBand(bool needsTargets)
    {
        int slots = needsTargets ? 3 : 2;

        if (_surfaceDropdown != null)
        {
            _surfaceDropdown.SetSlot(0, slots);
        }

        if (_modeDropdown != null)
        {
            _modeDropdown.SetSlot(1, slots);
        }

        if (needsTargets && _targetDropdown != null)
        {
            _targetDropdown.SetSlot(2, slots);
        }
    }

    private static void SyncControls()
    {
        _isSyncing = true;

        if (_surfaceDropdown != null)
        {
            _surfaceDropdown.SetValue(Mathf.Max(0, SurfaceOrder.IndexOf(_surface)));
        }

        if (_modeDropdown != null)
        {
            _modeDropdown.SetValue(SurfaceLook.ModeIndexOf(_surface));
        }

        if (_targetDropdown != null)
        {
            _targetDropdown.SetValue(_colorTarget);
        }

        Color color = SurfaceLook.ColorTarget(_surface, _colorTarget);
        int rowCount = ChannelCount + SurfaceLook.SliderCount(_surface);

        for (int index = 0; index < Rows.Count && index < rowCount; index++)
        {
            if (index < ChannelCount)
            {
                Rows[index].SetValue(color[index] * ChannelScale);
                continue;
            }

            Rows[index].SetValue(SurfaceLook.SliderValue(_surface, index - ChannelCount));
        }

        _isSyncing = false;
    }

    private static string ChannelLabel(int channel)
    {
        if (channel == 0)
            return "Red";

        return channel == 1 ? "Green" : "Blue";
    }

    private static void ClampSurface()
    {
        if (_surface >= SurfaceLook.SurfaceCount)
        {
            _surface = 0;
        }
    }
}
