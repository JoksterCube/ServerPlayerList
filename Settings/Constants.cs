using BepInEx.Configuration;
using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace JoksterCube.ServerPlayerList.Settings;

internal static class Constants
{
    internal static class Plugin
    {
        internal const string ModName = "ServerPlayerList";
        internal const string ModVersion = "1.0.3";
        internal const string Author = "JoksterCube";
        internal const string ModGUID = $"{Author}.{ModName}";
        internal const string Description = "Display currently online player number and information.";
        internal const string Copyright = "Copyright ©  2025";
        internal const string Guid = "39da2684-0d87-45f0-9265-a09885c0b1ba";

        internal const string ConfigFileName = $"{ModGUID}.cfg";
    }

    internal static class DisplayMessages
    {

    }

    internal static class DebugMessages
    {
        internal const string ReadConfigCalled = "ReadConfigValues called";
        internal static readonly string ErrorLoadingConfig = $"There was an issue loading your {Plugin.ConfigFileName}";
        internal const string RequestCheckConfig = "Please check your config entries for spelling and format!";
    }

    internal static class Groups
    {
        internal static class General
        {
            internal const string Group = "1 - General";

            internal static readonly ConfigInfo<Toggle> Lock = new(
                Group,
                "Lock Configuration",
                "If on, the configuration is locked and can be changed by server admins only.",
                Toggle.On,
                true);

            internal static readonly ConfigInfo<Toggle> ShowPlayers = new(
                Group,
                "Show Players",
                "Show online player list.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<float> RefreshDelay = new(
                Group,
                "Refresh Delay",
                "Time in seconds in bedtween refreshes.",
                .25f,
                true);
        }

        internal static class Appearance
        {
            internal const string Group = "2 - Appearance";

            internal static readonly ConfigInfo<Vector2> AnchorPosition = new(
                Group,
                "Anchor Position",
                "Position for the Server Player list view.",
                new(720.25f, 296.75f),
                false);

            internal static readonly ConfigInfo<float> Width = new(
                Group,
                "Width",
                "Width of the currently online panel.",
                199.5f,
                false);

            internal static readonly ConfigInfo<Color> BackgroundColor = new(
                Group,
                "Background Color",
                "Color of the panel background.",
                new(0, 0, 0, .3f),
                false);

            internal static readonly ConfigInfo<Color> HeaderTextColor = new(
                Group,
                "Header Text Color",
                "Color of the header text.",
                Color.white,
                false);

            internal static readonly ConfigInfo<Color> HeaderHighlightTextColor = new(
                Group,
                "Header Highlight Text Color",
                "Highlight color of the header text.",
                new Color(1, .84f, 0, 1),
                false);

            internal static readonly ConfigInfo<int> HeaderFontSize = new(
                Group,
                "Header Font Size",
                "Size of the header font.",
                28,
                false);

            internal static readonly ConfigInfo<int> ListFontSize = new(
                Group,
                "List Font Size",
                "Size of the lsit font.",
                22,
                false);

            internal static readonly ConfigInfo<string> HeaderText = new(
                Group,
                "Header Text",
                "Text going before the number of players currently online.",
                "Currently online:",
                false);
        }

        internal static class Inputs
        {
            internal const string Group = "3 - Inputs";

            internal static readonly ConfigInfo<KeyboardShortcut> ShowListKeyboardShortcut = new(
                Group,
                "Show List Keyboard shortcut",
                "Input used to display online player list.",
                new(KeyCode.O, KeyCode.RightControl),
                false);
        }
    }

    internal static class GameObjectNames
    {
        internal const string ServerPlayerListMain = "JoksterCube.ServerPlayerList.Main";
        internal const string ServerPlayerListBackground = "JoksterCube.ServerPlayerList.Background";
        internal const string ServerPlayerListHeader = "JoksterCube.ServerPlayerList.Header";
        internal const string ServerPlayerListContainer = "JoksterCube.ServerPlayerList.Container";
        internal const string ServerPlayerListPlayerInfo = "JoksterCube.ServerPlayerList.PlayerInfo";
        internal const string ServerPlayerListPlayerName = "JoksterCube.ServerPlayerList.PlayerName";
        internal const string ServerPlayerListDistance = "JoksterCube.ServerPlayerList.Distance";
    }

    internal static readonly Dictionary<PlayerDistance, Color> DistanceColors = new()
    {
        { PlayerDistance.Close, new(0f, 1f, 0f) },
        { PlayerDistance.Medium, new(1f, 1f, 0f) },
        { PlayerDistance.Far, new(1f, .65f, 0f) },
        { PlayerDistance.VeryFar, new(1f, .2f, .2f) },
        { PlayerDistance.Distant, new(.8f, .8f, .85f) },
    };
}
