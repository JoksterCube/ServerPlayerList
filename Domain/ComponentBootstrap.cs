using JoksterCube.ServerPlayerList.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;
using static JoksterCube.ServerPlayerList.Settings.Constants;

namespace JoksterCube.ServerPlayerList.Domain;

internal static class ComponentBootstrap
{
    private static ServerPlayerListInterfaceComponent _interface;

    internal static void Ensure()
    {
        if (_interface) return;
        var go = new GameObject(GameObjectNames.ServerPlayerListMain, typeof(RectTransform), typeof(ServerPlayerListInterfaceComponent));
        Object.DontDestroyOnLoad(go);

        _interface = go.GetComponent<ServerPlayerListInterfaceComponent>();
    }

    internal static void Clear()
    {
        if (!_interface) return;
        Object.Destroy(_interface.gameObject);
        _interface = null;
    }
}
