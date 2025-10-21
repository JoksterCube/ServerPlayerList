using BepInEx.Configuration;
using JoksterCube.ServerPlayerList.Common;
using ServerSync;
using TMPro;
using UnityEngine;
using static JoksterCube.ServerPlayerList.Settings.Constants.Groups;

namespace JoksterCube.ServerPlayerList.Settings;

internal static class PluginConfig
{
    private static ConfigEntry<Toggle> _serverConfigLocked = null!;

    internal static ConfigEntry<Toggle> ShowPlayers = null!;

    internal static ConfigEntry<float> RefreshDelay = null!;

    internal static ConfigEntry<Vector2> AnchorPosition = null!;
    internal static ConfigEntry<float> Width = null!;

    internal static ConfigEntry<Color> BackgroundColor = null!;
    internal static ConfigEntry<Color> HeaderTextColor = null!;
    internal static ConfigEntry<Color> HeaderHighlightTextColor = null!;

    internal static ConfigEntry<int> HeaderFontSize = null!;
    internal static ConfigEntry<int> ListFontSize = null!;

    internal static ConfigEntry<string> HeaderText = null!;

    internal static ConfigEntry<KeyboardShortcut> ShowListKeyboardShortcut = null!;

    internal static void Build(ConfigFile config, ConfigSync configSync)
    {
        ConfigOptions.Initialize(config, configSync);

        _serverConfigLocked = ConfigOptions.Config(General.Lock);
        configSync.AddLockingConfigEntry(_serverConfigLocked);

        ShowPlayers = ConfigOptions.Config(General.ShowPlayers);

        RefreshDelay = ConfigOptions.Config(General.RefreshDelay);

        AnchorPosition = ConfigOptions.Config(Appearance.AnchorPosition);
        Width = ConfigOptions.Config(Appearance.Width);

        BackgroundColor = ConfigOptions.Config(Appearance.BackgroundColor);
        HeaderTextColor = ConfigOptions.Config(Appearance.HeaderTextColor);
        HeaderHighlightTextColor = ConfigOptions.Config(Appearance.HeaderHighlightTextColor);

        HeaderFontSize = ConfigOptions.Config(Appearance.HeaderFontSize);
        ListFontSize = ConfigOptions.Config(Appearance.ListFontSize);

        HeaderText = ConfigOptions.Config(Appearance.HeaderText);

        ShowListKeyboardShortcut = ConfigOptions.Config(Inputs.ShowListKeyboardShortcut);
    }
}