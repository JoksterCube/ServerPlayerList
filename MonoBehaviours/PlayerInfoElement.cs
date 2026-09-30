using System;
using JoksterCube.ServerPlayerList.Domain;
using JoksterCube.ServerPlayerList.Settings;
using JoksterCube.ServerPlayerList.Common;
using TMPro;
using UnityEngine;
using static JoksterCube.ServerPlayerList.Settings.Constants;

namespace JoksterCube.ServerPlayerList.MonoBehaviours;

internal class PlayerInfoElement : MonoBehaviour
{
    private ServerPlayerInfo? lastInfo;
    private Toggle lastUseKilometers;
    private Toggle lastShowPlayerDirection;
    private string? lastLocalPlayerTag;
    private Color _defaultNameColor;
    private bool _hasDefaultNameColor;
    private Color _lastFavoriteNameColor;

    internal ServerPlayerInfo? PlayerInfo { get; set; }
    internal TMP_Text Name { get; set; } = null!;
    internal TMP_Text Distance { get; set; } = null!;
    internal event Action? FavoriteChanged;

    internal void SetPlayerInfo(ServerPlayerInfo playerInfo)
    {
        PlayerInfo = playerInfo;
        UpdateDisplay();
    }

    internal void ToggleFavorite()
    {
        if (PlayerInfo is null) return;

        ServerPlayerTracker.ToggleFavorite(PlayerInfo.Name);
        FavoriteChanged?.Invoke();
    }

    private void Update() => UpdateDisplay();

    private void UpdateDisplay()
    {
        var playerInfo = PlayerInfo;
        var useKilometers = PluginConfig.UseKilometers.Value;
        var showPlayerDirection = PluginConfig.ShowPlayerDirection.Value;
        var localPlayerTag = PluginConfig.LocalPlayerTag.Value;
        var favoriteNameColor = PluginConfig.FavoritePlayerNameColor.Value;
        if (!_hasDefaultNameColor)
        {
            _defaultNameColor = Name.color;
            _hasDefaultNameColor = true;
        }

        if (playerInfo is null || (playerInfo == lastInfo && useKilometers == lastUseKilometers && showPlayerDirection == lastShowPlayerDirection && localPlayerTag == lastLocalPlayerTag && favoriteNameColor == _lastFavoriteNameColor)) return;

        Name.text = playerInfo.Name;
        Name.color = playerInfo.IsFavorite ? favoriteNameColor : _defaultNameColor;
        Distance.color = DistanceColor(playerInfo);
        Distance.text = playerInfo.IsMe
            ? localPlayerTag
            : $"{(showPlayerDirection.IsOn() ? playerInfo.Direction : string.Empty)} {FormatDistance(playerInfo, useKilometers.IsOn())}".Trim();

        lastInfo = playerInfo;
        lastUseKilometers = useKilometers;
        lastShowPlayerDirection = showPlayerDirection;
        lastLocalPlayerTag = localPlayerTag;
        _lastFavoriteNameColor = favoriteNameColor;
    }

    private static Color DistanceColor(ServerPlayerInfo playerInfo) => DistanceColors[playerInfo.DistancIndicator];

    private static string FormatDistance(ServerPlayerInfo playerInfo, bool useKilometers)
    {
        if (!playerInfo.IsPublic) return "N/A";
        if (useKilometers && playerInfo.Distance >= 1000)
            return $"{playerInfo.Distance / 1000:F2} km";

        return playerInfo.Distance switch
        {
            >= 100 => $"{playerInfo.Distance:F0} m",
            >= 10 => $"{playerInfo.Distance:F1} m",
            _ => $"{playerInfo.Distance:F2} m"
        };
    }
}
