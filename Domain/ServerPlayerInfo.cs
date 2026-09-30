using UnityEngine;
using JoksterCube.ServerPlayerList.Settings;

namespace JoksterCube.ServerPlayerList.Domain;

internal class ServerPlayerInfo
{
    private static readonly string[] DirectionArrows = { "\u2191", "\u2197", "\u2192", "\u2198", "\u2193", "\u2199", "\u2190", "\u2196" };

    internal string Name { get; }
    internal float Distance { get; }
    internal string Direction { get; }
    public bool IsPublic { get; }
    internal bool IsMe { get; }
    internal bool IsFavorite { get; }

    internal PlayerDistance DistancIndicator => Distance switch
    {
        < 100 => PlayerDistance.Close,
        < 400 => PlayerDistance.Medium,
        < 1200 => PlayerDistance.Far,
        < 2400 => PlayerDistance.VeryFar,
        _ => PlayerDistance.Distant
    };

    internal ServerPlayerInfo(string name, float distance, bool isPublic, bool isMe = false, string direction = "", bool isFavorite = false)
    {
        Name = name;
        Distance = distance;
        IsPublic = isPublic;
        IsMe = isMe;
        Direction = direction;
        IsFavorite = isFavorite;
    }

    internal ServerPlayerInfo(ZNet.PlayerInfo info, bool isFavorite = false) : this(info.m_name.Trim(), GetDistance(info), IsPublicPosition(info), IsLocalPlayer(info), GetDirection(info), isFavorite) { }

    private static string GetDirection(ZNet.PlayerInfo info)
    {
        var me = Player.m_localPlayer;
        if (!me || !info.m_publicPosition || IsLocalPlayer(info)) return string.Empty;

        var offset = info.m_position - me.transform.position;
        if (offset.sqrMagnitude < 0.01f) return DirectionArrows[0];

        var targetAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        var referenceYaw = me.transform.eulerAngles.y;
        if (PluginConfig.DirectionReference.Value == DirectionReference.Camera && Camera.main)
            referenceYaw = Camera.main.transform.eulerAngles.y;

        var relativeAngle = Mathf.DeltaAngle(referenceYaw, targetAngle);
        var directionIndex = (Mathf.RoundToInt(relativeAngle / 45f) + DirectionArrows.Length) % DirectionArrows.Length;
        return DirectionArrows[directionIndex];
    }

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
