using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.MonoBehaviours;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static JoksterCube.ServerPlayerList.Settings.PluginConfig;

namespace JoksterCube.ServerPlayerList.Domain;

internal static class InputManager
{
    internal static void Update(Plugin plugin)
    {
        if (!ShowListKeyboardShortcut.Value.IsKeyDown()) return;
        if (!ServerPlayerListInterfaceComponent.ShouldBeVisible() || IsTyping()) return;

        ShowPlayers.Value = ShowPlayers.Value.Not();
        plugin.Config.Save();
    }

    private static bool IsTyping() =>
        (Chat.instance && Chat.instance.HasFocus())
        || Console.IsVisible()
        || TextInput.IsVisible()
        || Menu.IsVisible()
        || InventoryGui.IsVisible()
        || StoreGui.IsVisible()
        || IsInputFieldFocused();

    private static bool IsInputFieldFocused()
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem) return false;

        var selected = eventSystem.currentSelectedGameObject;
        if (!selected) return false;

        var tmp = selected.GetComponent<TMP_InputField>();
        if (tmp && tmp.isFocused) return true;

        var legacy = selected.GetComponent<InputField>();
        return legacy && legacy.isFocused;
    }
}
