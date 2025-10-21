using BepInEx.Configuration;
using System.Linq;
using UnityEngine;

namespace JoksterCube.ServerPlayerList.Common;

internal static class KeyboardExtensions
{
    internal static bool IsKeyDown(this KeyboardShortcut shortcut) =>
        shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);

    internal static bool IsKeyHeld(this KeyboardShortcut shortcut) =>
        shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
}
