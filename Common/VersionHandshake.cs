using System;
using System.Collections.Generic;
using HarmonyLib;
using static JoksterCube.ServerPlayerList.Settings.Constants.Plugin;

namespace JoksterCube.ServerPlayerList.Common;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
internal static class RegisterAndCheckVersion
{
    private static void Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        // Register version check call
        Plugin.ModLogger.LogDebug("Registering version RPC handler");
        peer.m_rpc.Register($"{ModName}_VersionCheck",
            new Action<ZRpc, ZPackage>(RpcHandlers.RPC_ServerSyncModTemplate_Version));

        // Make calls to check versions
        Plugin.ModLogger.LogInfo("Invoking version check");
        ZPackage zpackage = new();
        zpackage.Write(ModVersion);
        peer.m_rpc.Invoke($"{ModName}_VersionCheck", zpackage);
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
internal static class VerifyClient
{
    private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
    {
        if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
        // Disconnect peer if they didn't send mod version at all
        Plugin.ModLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
        rpc.Invoke("Error", 3);
        return false; // Prevent calling underlying method
    }

    private static void Postfix(ZNet __instance) =>
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), $"{ModName}RequestAdminSync", new ZPackage());
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError))]
internal class ShowConnectionError
{
    private static void Postfix(FejdStartup __instance)
    {
        if (__instance.m_connectionFailedPanel.activeSelf)
        {
            __instance.m_connectionFailedError.fontSizeMax = 25;
            __instance.m_connectionFailedError.fontSizeMin = 15;
            __instance.m_connectionFailedError.text += $"\n{Plugin.ConnectionError}";
        }
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
internal static class RemoveDisconnectedPeerFromVerified
{
    private static void Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        if (!__instance.IsServer()) return;
        // Remove peer from validated list
        Plugin.ModLogger.LogInfo($"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
        _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
    }
}

internal static class RpcHandlers
{
    internal static readonly List<ZRpc> ValidatedPeers = new();

    internal static void RPC_ServerSyncModTemplate_Version(ZRpc rpc, ZPackage pkg)
    {
        string version = pkg.ReadString();
        Plugin.ModLogger.LogInfo($"Version check, local: {ModVersion},  remote: {version}");
        if (version != ModVersion)
        {
            Plugin.ConnectionError = $"{ModName} Installed: {ModVersion}\n Needed: {version}";
            if (!ZNet.instance.IsServer()) return;
            // Different versions - force disconnect client from server
            Plugin.ModLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
            rpc.Invoke("Error", 3);
        }
        else
        {
            if (!ZNet.instance.IsServer())
            {
                // Enable mod on client if versions match
                Plugin.ModLogger.LogInfo("Received same version from server!");
            }
            else
            {
                // Add client to validated list
                Plugin.ModLogger.LogInfo($"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                ValidatedPeers.Add(rpc);
            }
        }
    }
}