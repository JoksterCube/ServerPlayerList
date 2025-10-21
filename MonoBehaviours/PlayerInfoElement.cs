using HarmonyLib;
using JoksterCube.ServerPlayerList.Domain;
using JoksterCube.ServerPlayerList.Settings;
using TMPro;
using UnityEngine;
using static JoksterCube.ServerPlayerList.Settings.Constants;

namespace JoksterCube.ServerPlayerList.MonoBehaviours;

internal class PlayerInfoElement : MonoBehaviour
{
    internal ServerPlayerInfo PlayerInfo { get; set; }
    internal TMP_Text Name { get; set; }
    internal TMP_Text Distance { get; set; }

    private ServerPlayerInfo lastInfo = null;

    private void Update()
    {
        if (PlayerInfo == lastInfo) return;

        Name.text = PlayerInfo.Name;
        Distance.color = DistanceColor();
        Distance.text = FromatDistance();

        lastInfo = PlayerInfo;
    }

    private Color DistanceColor() => DistanceColors[PlayerInfo.DistancIndicator];

    private string FromatDistance()
    {
        if (PlayerInfo.IsMe) return string.Empty;
        if (!PlayerInfo.IsPublic) return "N/A";

        return PlayerInfo.Distance switch
        {
            >= 100 => $"{PlayerInfo.Distance:F0}m",
            >= 10 => $"{PlayerInfo.Distance:F1}m",
            _ => $"{PlayerInfo.Distance:F2}m"
        };
    }
}
