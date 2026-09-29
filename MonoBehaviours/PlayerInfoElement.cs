using JoksterCube.ServerPlayerList.Domain;
using JoksterCube.ServerPlayerList.Settings;
using JoksterCube.ServerPlayerList.Common;
using TMPro;
using UnityEngine;
using static JoksterCube.ServerPlayerList.Settings.Constants;

namespace JoksterCube.ServerPlayerList.MonoBehaviours;

internal class PlayerInfoElement : MonoBehaviour
{
    internal ServerPlayerInfo? PlayerInfo { get; set; }
    internal TMP_Text Name { get; set; } = null!;
    internal TMP_Text Distance { get; set; } = null!;

    private ServerPlayerInfo? lastInfo;
    private Toggle lastUseKilometers;
    private string? lastLocalPlayerTag;

    private void Update()
    {
        var playerInfo = PlayerInfo;
        var useKilometers = PluginConfig.UseKilometers.Value;
        var localPlayerTag = PluginConfig.LocalPlayerTag.Value;
        if (playerInfo is null || (playerInfo == lastInfo && useKilometers == lastUseKilometers && localPlayerTag == lastLocalPlayerTag)) return;

        Name.text = playerInfo.Name;
        Distance.color = DistanceColor(playerInfo);
        Distance.text = playerInfo.IsMe
            ? localPlayerTag == "\u265b" && !Distance.font.HasCharacter('\u265b')
                ? Distance.font.HasCharacter('\u2605') ? "\u2605" : "@"
                : localPlayerTag
            : FormatDistance(playerInfo);

        lastInfo = playerInfo;
        lastUseKilometers = useKilometers;
        lastLocalPlayerTag = localPlayerTag;
    }

    private static Color DistanceColor(ServerPlayerInfo playerInfo) => DistanceColors[playerInfo.DistancIndicator];

    private static string FormatDistance(ServerPlayerInfo playerInfo)
    {
        if (!playerInfo.IsPublic) return "N/A";
        if (PluginConfig.UseKilometers.IsOn() && playerInfo.Distance >= 1000)
            return $"{playerInfo.Distance / 1000:F2} km";

        return playerInfo.Distance switch
        {
            >= 100 => $"{playerInfo.Distance:F0} m",
            >= 10 => $"{playerInfo.Distance:F1} m",
            _ => $"{playerInfo.Distance:F2} m"
        };
    }
}
