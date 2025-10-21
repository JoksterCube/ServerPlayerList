using System;
using UnityEngine;

namespace JoksterCube.ServerPlayerList.Domain;

internal class ServerPlayerInfo
{
    internal string Name { get; }
    internal float Distance { get; }
    public bool IsPublic { get; }
    internal bool IsMe { get; }

    internal PlayerDistance DistancIndicator => Distance switch
    {
        < 100 => PlayerDistance.Close,
        < 400 => PlayerDistance.Medium,
        < 1200 => PlayerDistance.Far,
        < 2400 => PlayerDistance.VeryFar,
        _ => PlayerDistance.Distant
    };

    internal ServerPlayerInfo(string name, float distance, bool isPublic, bool isMe = false)
    {
        Name = name;
        Distance = distance;
        IsPublic = isPublic;
        IsMe = isMe;
    }

    internal ServerPlayerInfo(ZNet.PlayerInfo info) : this(info.m_name.Trim(), GetDistance(info), IsPublicPosition(info), IsLocalPlayer(info)) { }

    private static bool IsLocalPlayer(ZNet.PlayerInfo info)
    {
        var me = Player.m_localPlayer;
        if (!me) return false;

        var myId = me.GetZDOID();
        return info.m_characterID == myId;
    }

    private static float GetDistance(ZNet.PlayerInfo info)
    {
        if (IsLocalPlayer(info)) return float.NegativeInfinity;
        if (!info.m_publicPosition) return float.PositiveInfinity;
        return Vector3.Distance(Player.m_localPlayer.transform.position, info.m_position);
    }
    private static bool IsPublicPosition(ZNet.PlayerInfo info) =>
        info.m_publicPosition;
}
