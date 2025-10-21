using HarmonyLib;
using JoksterCube.ServerPlayerList.Domain;
using System.Collections;

namespace JoksterCube.ServerPlayerList.Patches;

[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
internal class ZNetSceneAwakePatch
{
    static void Postfix(ZNetScene __instance) =>
        __instance.StartCoroutine(EnsureAfterHud());

    private static IEnumerator EnsureAfterHud()
    {
        while (Hud.instance == null || Hud.instance.m_rootObject == null) yield return null;
        yield return null;
        ComponentBootstrap.Ensure();
    }
}
