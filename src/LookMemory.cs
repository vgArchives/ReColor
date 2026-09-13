using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RestoryReColor;

internal static class LookMemory
{
    private const string FileName = "RestoryReColor.look.json";

    private static readonly Dictionary<string, float[]> LookBySurface = new Dictionary<string, float[]>();

    private static bool _hasRead;

    [Serializable]
    private class StoredLook
    {
        public List<StoredSurface> Surfaces = new List<StoredSurface>();
    }

    [Serializable]
    private class StoredSurface
    {
        public string Name;
        public float[] State;
    }

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    internal static bool TryRecall(string surfaceName, out float[] state)
    {
        EnsureRead();

        return LookBySurface.TryGetValue(surfaceName, out state);
    }

    internal static void Remember(string surfaceName, float[] state)
    {
        EnsureRead();

        LookBySurface[surfaceName] = state;
    }

    internal static void Write()
    {
        var stored = new StoredLook();

        foreach (KeyValuePair<string, float[]> look in LookBySurface)
        {
            stored.Surfaces.Add(new StoredSurface { Name = look.Key, State = look.Value });
        }

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(stored, true));

            Log.Debug($"ReColor: wrote the look of {stored.Surfaces.Count} surface(s) to \"{FilePath}\".");
        }
        catch (Exception failure)
        {
            Log.Warning($"ReColor: the look could not be saved to \"{FilePath}\": {failure.Message}");
        }
    }

    private static void EnsureRead()
    {
        if (_hasRead)
            return;

        _hasRead = true;

        if (!File.Exists(FilePath))
        {
            Log.Debug("ReColor: no stored look yet; the bench starts on the game's own values.");
            return;
        }

        try
        {
            var stored = JsonUtility.FromJson<StoredLook>(File.ReadAllText(FilePath));

            if (stored?.Surfaces == null)
            {
                Log.Warning($"ReColor: \"{FilePath}\" holds no look; the bench starts on the game's values.");
                return;
            }

            foreach (StoredSurface surface in stored.Surfaces)
            {
                bool isUsable = !string.IsNullOrEmpty(surface?.Name) && surface.State != null;

                if (isUsable)
                {
                    LookBySurface[surface.Name] = surface.State;
                }
            }

            Log.Debug($"ReColor: read the look of {LookBySurface.Count} surface(s).");
        }
        catch (Exception failure)
        {
            Log.Warning($"ReColor: \"{FilePath}\" could not be read: {failure.Message}");
        }
    }
}
