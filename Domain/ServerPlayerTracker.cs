using System.Collections.Generic;
using System.Linq;

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

        _players = ZNet.instance.GetPlayerList();
    }
}
