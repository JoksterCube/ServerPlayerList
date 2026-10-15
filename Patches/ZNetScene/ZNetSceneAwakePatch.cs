using HarmonyLib;
using JoksterCube.ServerPlayerList.Domain;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace JoksterCube.ServerPlayerList.Patches;

[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
internal class ZNetSceneAwakePatch
{
    private static void Postfix(ZNetScene __instance)
    {
        // Headless dedicated servers never create a HUD.
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        __instance.StartCoroutine(EnsureAfterHud());
    }

    private static IEnumerator EnsureAfterHud()
    {
        while (Hud.instance == null || Hud.instance.m_rootObject == null) yield return null;
        yield return null;
        ComponentBootstrap.Ensure();
    }
}
