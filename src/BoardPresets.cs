using System;
using UnityEngine;

namespace ReColor;

internal static class BoardPresets
{
    internal sealed class Preset
    {
        internal string Name;
        internal int Mode;
        internal int Rgb;
        internal float[] Sliders;
    }

    private sealed class SurfaceStyle
    {
        internal string SurfaceName;
        internal Func<string> ShortName;
        internal Preset[] Presets;
    }

    private const int Original = 0;
    private const int TableFlat = 1;
    private const int Greyscale = 1;
    private const int RugFlat = 2;

    private static readonly Preset[] Table =
    {
        DefinePreset("Golden oak", Original, 0xD9A34A, 0f, 0.20f, 0f),
        DefinePreset("Special walnut", Original, 0x6B4423, 0f, 0.30f, 0.05f),
        DefinePreset("Red mahogany", Original, 0x8C3A2B, 0f, 0.45f, 0.10f),
        DefinePreset("Ebony", Original, 0x2A2420, 0f, 0.55f, 0.15f),
        DefinePreset("Weathered grey", Original, 0x9A9186, 0f, 0.10f, 0f),
        DefinePreset("Mint lacquer", TableFlat, 0x7FD1AE, 0f, 0.85f, 0.10f),
        DefinePreset("Coral enamel", TableFlat, 0xFF6B5B, 0f, 0.80f, 0.05f),
        DefinePreset("Brushed steel", TableFlat, 0xB8C0C8, 0f, 0.70f, 0.90f),
        DefinePreset("Polished brass", TableFlat, 0xC9972B, 0f, 0.65f, 0.85f),
        DefinePreset("Arcade indigo", TableFlat, 0x2B1B5E, 1.2f, 0.90f, 0.30f)
    };

    private static readonly Preset[] NormalRug =
    {
        DefinePreset("Cream", Greyscale, 0xF2E5D5, 0.3f),
        DefinePreset("Oat", Greyscale, 0xD9C7A7, 0.4f),
        DefinePreset("Moss", Greyscale, 0x5E6B4A, 0.7f),
        DefinePreset("Dusty rose", Greyscale, 0xC98B8B, 0.6f),
        DefinePreset("Denim", Greyscale, 0x4A6FA5, 1.0f),
        DefinePreset("Plum", Greyscale, 0x6B4E71, 1.3f),
        DefinePreset("Charcoal", Greyscale, 0x33333A, 1.8f),
        DefinePreset("Terracotta", RugFlat, 0xB5563C, 0.8f),
        DefinePreset("Cyan panel", RugFlat, 0x00F0FF, 0.8f),
        DefinePreset("Magenta panel", RugFlat, 0xFF2D95, 0.8f)
    };

    private static readonly Preset[] TournamentRug =
    {
        DefinePreset("Hot magenta", Greyscale, 0xFF2D95, 1.1f, 1.5f, 2.0f),
        DefinePreset("Electric cyan", Greyscale, 0x00F0FF, 1.1f, 1.5f, 2.2f),
        DefinePreset("Laser grid", Greyscale, 0x12002E, 0.8f, 2.2f, 3.0f),
        DefinePreset("Coin op gold", Greyscale, 0xFFC107, 1.2f, 1.3f, 2.6f),
        DefinePreset("Neon lime", Greyscale, 0x31CC5D, 1.15f, 1.45f, 1.8f),
        DefinePreset("Deep indigo", Greyscale, 0x613FD1, 1.0f, 1.7f, 2.4f),
        DefinePreset("Sunset grid", Greyscale, 0xF35F0F, 1.2f, 1.4f, 2.0f),
        DefinePreset("Vapor peach", RugFlat, 0xFFB8A0, 1.4f, 0.9f, 1.2f),
        DefinePreset("Cotton candy", RugFlat, 0xC4A3FF, 1.5f, 0.8f, 1.0f),
        DefinePreset("Blackout", RugFlat, 0x0A0A12, 0.7f, 2.6f, 3.6f)
    };

    private static readonly Preset[] NoPresets = new Preset[0];

    private static readonly SurfaceStyle[] Styles =
    {
        DefineStyle(SurfaceLook.NormalRugSurfaceName, () => Strings.Normal, NormalRug),
        DefineStyle(SurfaceLook.TournamentSurfaceName, () => Strings.Tournament, TournamentRug),
        DefineStyle(SurfaceLook.TableSurfaceName, () => Strings.Table, Table)
    };

    internal static Preset[] For(string surfaceName)
    {
        SurfaceStyle style = StyleOf(surfaceName);

        return style == null ? NoPresets : style.Presets;
    }

    internal static string ShortNameOf(string surfaceName)
    {
        SurfaceStyle style = StyleOf(surfaceName);

        return style == null ? surfaceName : style.ShortName();
    }

    internal static Color ColorOf(Preset preset)
    {
        float red = ((preset.Rgb >> 16) & 0xFF) / 255f;
        float green = ((preset.Rgb >> 8) & 0xFF) / 255f;
        float blue = (preset.Rgb & 0xFF) / 255f;

        return new Color(red, green, blue, 1f);
    }

    private static SurfaceStyle StyleOf(string surfaceName)
    {
        foreach (SurfaceStyle style in Styles)
        {
            if (style.SurfaceName == surfaceName)
                return style;
        }

        return null;
    }

    private static Preset DefinePreset(string name, int mode, int rgb, params float[] sliders)
    {
        return new Preset { Name = name, Mode = mode, Rgb = rgb, Sliders = sliders };
    }

    private static SurfaceStyle DefineStyle(string surfaceName, Func<string> shortName, Preset[] presets)
    {
        return new SurfaceStyle { SurfaceName = surfaceName, ShortName = shortName, Presets = presets };
    }
}
