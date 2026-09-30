using System;
using System.Collections.Generic;
using System.Linq;
using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Settings;

namespace JoksterCube.ServerPlayerList.Domain;

internal static class ServerPlayerTracker
{
    private static List<ZNet.PlayerInfo> _players = [];
    private static int _currentlyOnline;

    internal static int CurrentlyOnline =>
        _currentlyOnline;

    internal static void ToggleFavorite(string playerName)
    {
        var favoritePlayers = PluginConfig.FavoritePlayers.Value
            .Split(',')
            .Select(name => name.Trim())
            .Where(name => name.Length > 0)
            .ToList();
        var removed = favoritePlayers.RemoveAll(name => string.Equals(name, playerName, StringComparison.Ordinal)) > 0;

        if (!removed)
            favoritePlayers.Add(playerName);

        PluginConfig.FavoritePlayers.Value = string.Join(", ", favoritePlayers);
    }

    internal static List<ServerPlayerInfo> GetCurrenlyOnlineList()
    {
        var favoritePlayers = new HashSet<string>(
            PluginConfig.FavoritePlayers.Value.Split(',').Select(name => name.Trim()).Where(name => name.Length > 0));
        var hideLocalPlayer = PluginConfig.HideLocalPlayer.Value.IsOn();
        var hideNaPlayers = PluginConfig.HideNaPlayers.Value.IsOn();
        var maxDistance = PluginConfig.MaxPlayerDistance.Value;
        var maxVisiblePlayers = PluginConfig.MaxVisiblePlayers.Value;

        var playerInfos = _players
            .Select(x => new ServerPlayerInfo(x, favoritePlayers.Contains(x.m_name.Trim())))
            .Where(info => info.IsFavorite || (
                (!hideLocalPlayer || !info.IsMe)
                && (!hideNaPlayers || info.IsMe || info.IsPublic)
                && (maxDistance <= 0 || info.Distance <= maxDistance)))
            .OrderByDescending(info => info.IsMe)
            .ThenByDescending(info => info.IsFavorite)
            .ThenBy(info => info.Distance)
            .ThenBy(info => info.Name)
            .ToList();

        if (maxVisiblePlayers <= 0)
            return playerInfos;

        var regularRowLimit = Math.Max(0, maxVisiblePlayers - playerInfos.Count(info => info.IsFavorite));
        var regularRows = new HashSet<ServerPlayerInfo>(
            playerInfos.Where(info => !info.IsFavorite).Take(regularRowLimit));

        return playerInfos
            .Where(info => info.IsFavorite || regularRows.Contains(info))
            .ToList();
    }

    internal static void UpdatePlayerInfo()
    {
        if (!ZNet.instance) return;

        var rconName = PluginConfig.IgnoreRconUser.IsOn() ? Plugin.RconPlayerName.Value : string.Empty;
        _players = ZNet.instance.GetPlayerList().ToList();
        if (!string.IsNullOrEmpty(rconName))
            RemovePlayer(rconName, requirePrivatePosition: true);

        foreach (var entry in PluginConfig.IgnoredUsers.Value.Split(','))
        {
            var name = entry.Trim();
            if (name.Length > 0)
                RemovePlayer(name, requirePrivatePosition: false);
        }

        _currentlyOnline = _players.Count;
    }

    internal static void UpdatePlayerCount()
    {
        if (!ZNet.instance) return;

        _players.Clear();
        var players = ZNet.instance.GetPlayerList();
        var removedPlayers = new bool[players.Count];
        var removedCount = 0;

        var rconName = PluginConfig.IgnoreRconUser.IsOn() ? Plugin.RconPlayerName.Value : string.Empty;
        if (!string.IsNullOrEmpty(rconName))
        {
            var index = FindPlayerIndex(players, removedPlayers, rconName, requirePrivatePosition: true, skipLocalPlayer: true);
            if (index >= 0)
            {
                removedPlayers[index] = true;
                removedCount++;
            }
        }

        foreach (var entry in PluginConfig.IgnoredUsers.Value.Split(','))
        {
            var name = entry.Trim();
            if (name.Length == 0) continue;

            var index = FindPlayerIndex(players, removedPlayers, name, requirePrivatePosition: true, skipLocalPlayer: true);
            if (index < 0)
                index = FindPlayerIndex(players, removedPlayers, name, requirePrivatePosition: false, skipLocalPlayer: false);

            if (index >= 0)
            {
                removedPlayers[index] = true;
                removedCount++;
            }
        }

        _currentlyOnline = players.Count - removedCount;
    }

    private static int FindPlayerIndex(List<ZNet.PlayerInfo> players, bool[] removedPlayers, string name, bool requirePrivatePosition, bool skipLocalPlayer)
    {
        var localPlayer = Player.m_localPlayer;
        var localPlayerId = localPlayer ? localPlayer.GetZDOID() : default;

        for (var index = 0; index < players.Count; index++)
        {
            var player = players[index];
            if (removedPlayers[index] || player.m_name != name) continue;
            if (requirePrivatePosition && player.m_publicPosition) continue;
            if (skipLocalPlayer && localPlayer && player.m_characterID == localPlayerId) continue;

            return index;
        }

        return -1;
    }

    private static void RemovePlayer(string name, bool requirePrivatePosition)
    {
        var index = _players.FindIndex(player =>
            player.m_name == name
            && !player.m_publicPosition
            && !new ServerPlayerInfo(player).IsMe);
        if (index < 0 && !requirePrivatePosition)
            index = _players.FindIndex(player => player.m_name == name);

        if (index >= 0)
            _players.RemoveAt(index);
    }
}
