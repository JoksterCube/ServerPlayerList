using BepInEx.Configuration;
using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace JoksterCube.ServerPlayerList.Settings;

internal enum DirectionReference
{
    Camera,
    Character
}

internal static class Constants
{
    internal static class Plugin
    {
        internal const string ModName = "ServerPlayerList";
        internal const string ModVersion = "1.2.1";
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

            internal static readonly ConfigInfo<float> RefreshDelay = new(
                Group,
                "Refresh Delay",
                "Time in seconds in bedtween refreshes.",
                .25f,
                true);
        }

        internal static class PlayerList
        {
            internal const string Group = "2 - Player List";

            internal static readonly ConfigInfo<Toggle> ShowPlayers = new(
                Group,
                "Show Players",
                "Show online player list.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<Toggle> HideLocalPlayer = new(
                Group,
                "Hide Local Player",
                "Hide your own non-favorite row from the expanded list. Does not affect the online player count.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<Toggle> HideNaPlayers = new(
                Group,
                "Hide Players with N/A Distance",
                "Hide non-favorite players who are not sharing their position from the expanded list. Does not affect the online player count.",
                Toggle.Off,
                false);

            internal static readonly ConfigInfo<Toggle> IgnoreRconUser = new(
                Group,
                "Ignore RCON user",
                "Hide one player matching the configured server chat name of ValheimRcon by Tristan-dvr that would show N/A distance. Requires ServerPlayerList and ValheimRcon on the server. A real player with the same name and a private position may be hidden instead.",
                Toggle.Off,
                true);

            internal static readonly ConfigInfo<string> IgnoredUsers = new(
                Group,
                "Ignored users",
                "Comma-separated player names to hide. Synced from the server when ServerPlayerList is installed there; otherwise uses your local setting. Each occurrence hides one exact, case-sensitive match, preferring N/A distance entries. Repeat a name to hide multiple players. Spaces around entries and empty entries are ignored. Applied after RCON filtering.",
                string.Empty,
                true);

            internal static readonly ConfigInfo<Toggle> UseKilometers = new(
                Group,
                "Use Kilometers",
                "Show distances of 1000 m or more in km instead of m.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<float> MaxPlayerDistance = new(
                Group,
                "Maximum Player Distance",
                new ConfigDescription("Hide non-favorite players farther than this distance in meters. Set to 0 to disable.", new AcceptableValueRange<float>(0, 20000)),
                0,
                false);

            internal static readonly ConfigInfo<int> MaxVisiblePlayers = new(
                Group,
                "Maximum Visible Players",
                new ConfigDescription("Maximum number of player rows to show. Favorites fill the limit first, and all favorites remain visible if they exceed it. Set to 0 for no limit.", new AcceptableValueRange<int>(0, 20)),
                0,
                false);

            internal static readonly ConfigInfo<string> FavoritePlayers = new(
                Group,
                "Favorite Players",
                "Comma-separated player names to always show while online, above other players. Favorites bypass local display filters and remain visible when their count exceeds the row limit; explicit ignored-user and RCON filters still apply. Matching is exact and case-sensitive.",
                string.Empty,
                false);

            internal static readonly ConfigInfo<Toggle> ShowPlayerDirection = new(
                Group,
                "Show Player Direction",
                "Show a relative direction arrow before the distance for players sharing their position.",
                Toggle.On,
                false);

            internal static readonly ConfigInfo<DirectionReference> DirectionReference = new(
                Group,
                "Direction Reference",
                "Choose whether direction arrows are relative to the camera or character facing direction.",
                Settings.DirectionReference.Camera,
                false);

            internal static readonly ConfigInfo<string> LocalPlayerTag = new(
                Group,
                "Local Player Tag",
                "Text shown instead of distance for your player. Use a symbol supported by the game font; leave empty to hide it.",
                "\u16dd",
                false);
        }

        internal static class Appearance
        {
            internal const string Group = "3 - Appearance";

            internal static readonly ConfigInfo<Vector2> AnchorPosition = new(
                Group,
                "Anchor Position",
                "Offset of the Server Player list view from the upper-right corner.",
                new(-239.75f, -243.25f),
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

            internal static readonly ConfigInfo<Color> FavoritePlayerNameColor = new(
                Group,
                "Favorite Player Name Color",
                "Name color used to identify favorite players in the list.",
                new Color(1f, .84f, 0f, 1f),
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
            internal const string Group = "4 - Inputs";

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
