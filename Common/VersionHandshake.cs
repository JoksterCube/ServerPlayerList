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

// Clients without the mod are allowed; only clients that reported a different version are blocked.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
internal static class BlockMismatchedClient
{
    private static bool Prefix(ZRpc rpc, ref ZNet __instance) =>
        !__instance.IsServer() || !RpcHandlers.RejectedPeers.Contains(rpc);
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
internal static class RemoveDisconnectedPeerFromRejected
{
    private static void Prefix(ZNetPeer peer, ref ZNet __instance)
    {
        if (!__instance.IsServer())
        {
            RpcHandlers.ResetServerVersionCheck();
            return;
        }

        _ = RpcHandlers.RejectedPeers.Remove(peer.m_rpc);
    }
}

internal static class RpcHandlers
{
    internal static readonly HashSet<ZRpc> RejectedPeers = [];
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
        Plugin.ConnectionError = string.Empty;
    }

    internal static void ResetServerVersionCheck()
    {
        _waitingForServerVersion = false;
        _serverVersionReceived = false;
    }

    // The remote version is shown in a rich-text label; strip markup and cap its length.
    private static string SanitizeRemoteText(string text)
    {
        var sanitized = text.Replace("<", string.Empty).Replace(">", string.Empty);
        return sanitized.Length > 32 ? sanitized.Substring(0, 32) : sanitized;
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
            Plugin.ConnectionError = $"{ModName} Installed: {ModVersion}\n Needed: {SanitizeRemoteText(version)}";
            if (!isServer) return;
            // Client has the mod but a different version - force disconnect client from server
            Plugin.ModLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
            RejectedPeers.Add(rpc);
            rpc.Invoke("Error", 3);
        }
        else if (!isServer)
        {
            Plugin.ModLogger.LogInfo("Received same version from server!");
        }
    }
}