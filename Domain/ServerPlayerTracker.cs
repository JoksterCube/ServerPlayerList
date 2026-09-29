using System.Collections.Generic;
using System.Linq;
using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Settings;

namespace JoksterCube.ServerPlayerList.Domain;

internal static class ServerPlayerTracker
{
    private static List<ZNet.PlayerInfo> _players = new();

    internal static int CurrentlyOnline =>
        _players.Count;

    internal static List<ServerPlayerInfo> GetCurrenlyOnlineList() =>
        _players
            .Select(x => new ServerPlayerInfo(x))
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Name)
            .ToList();

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
