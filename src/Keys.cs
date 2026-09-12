using UnityEngine;

namespace ReColor;

internal static class Keys
{
    internal static bool WasPressed(KeyCode key)
    {
        return key != KeyCode.None && Input.GetKeyDown(key);
    }
}
