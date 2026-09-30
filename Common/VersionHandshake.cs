using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static JoksterCube.ServerPlayerList.Settings.Constants.Plugin;

namespace JoksterCube.ServerPlayerList.Common;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
internal static class RegisterAndCheckVersion
{
    private static void Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        if (!__instance.IsServer())
            RpcHandlers.BeginServerVersionCheck();

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
        if (!__instance.IsServer())
        {
            RpcHandlers.ResetServerVersionCheck();
            return;
        }
        // Remove peer from validated list
        Plugin.ModLogger.LogInfo($"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
        _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
    }
}

internal static class RpcHandlers
{
    internal static readonly List<ZRpc> ValidatedPeers = [];
    private static bool _waitingForServerVersion;
    private static bool _serverVersionReceived;
    private static float _serverVersionCheckStartedAt;

    internal static bool ShouldWaitForInitialConfigSync
    {
        get
        {
            if (!ZNet.instance || ZNet.instance.IsServer() || !_waitingForServerVersion) return false;
            if (_serverVersionReceived) return !Plugin.InitialConfigSyncDone;

            return Time.realtimeSinceStartup - _serverVersionCheckStartedAt < 1f;
        }
    }

    internal static void BeginServerVersionCheck()
    {
        _waitingForServerVersion = true;
        _serverVersionReceived = false;
        _serverVersionCheckStartedAt = Time.realtimeSinceStartup;
    }

    internal static void ResetServerVersionCheck()
    {
        _waitingForServerVersion = false;
        _serverVersionReceived = false;
    }

    internal static void RPC_ServerSyncModTemplate_Version(ZRpc rpc, ZPackage pkg)
    {
        string version = pkg.ReadString();
        var instance = ZNet.instance;
        if (!instance) return;

        var isServer = instance.IsServer();
        if (!isServer)
            _serverVersionReceived = true;

        Plugin.ModLogger.LogInfo($"Version check, local: {ModVersion},  remote: {version}");
        if (version != ModVersion)
        {
            Plugin.ConnectionError = $"{ModName} Installed: {ModVersion}\n Needed: {version}";
            if (!isServer) return;
            // Different versions - force disconnect client from server
            Plugin.ModLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
            rpc.Invoke("Error", 3);
        }
        else
        {
            if (!isServer)
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