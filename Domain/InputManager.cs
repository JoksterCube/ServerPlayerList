using JoksterCube.ServerPlayerList.Common;
using static JoksterCube.ServerPlayerList.Settings.PluginConfig;

namespace JoksterCube.ServerPlayerList.Domain;

internal static class InputManager
{
    internal static void Update(Plugin plugin)
    {
        if (ShowListKeyboardShortcut.Value.IsKeyDown())
        {
            ShowPlayers.Value = ShowPlayers.Value.Not();

            plugin.Config.Save();
            return;
        }
    }
}
