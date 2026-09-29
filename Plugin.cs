using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JoksterCube.ServerPlayerList.Common;
using JoksterCube.ServerPlayerList.Domain;
using JoksterCube.ServerPlayerList.Settings;
using ServerSync;
using System.IO;
using System.Reflection;
using static JoksterCube.ServerPlayerList.Settings.Constants;
using static JoksterCube.ServerPlayerList.Settings.Constants.Plugin;

namespace JoksterCube.ServerPlayerList;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    internal static string ConnectionError = string.Empty;

    private readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony _harmony = new(ModGUID);

    public static readonly ManualLogSource ModLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    private static readonly ConfigSync ConfigSync = new(ModGUID)
    {
        DisplayName = ModName,
        CurrentVersion = ModVersion,
        MinimumRequiredVersion = ModVersion
    };

    internal static readonly CustomSyncedValue<string> RconPlayerName = new(ConfigSync, "RconPlayerName", string.Empty);

    private void Awake()
    {
        PluginConfig.Build(Config, ConfigSync);

        var assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
        SetupWatcher();
    }

    private void Update()
    {
        UpdateRconPlayerName();
        InputManager.Update(this);
    }

    private static void UpdateRconPlayerName()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;

        var name = string.Empty;
        if (PluginConfig.IgnoreRconUser.IsOn()
            && Chainloader.PluginInfos.TryGetValue("org.tristan.rcon", out var rcon)
            && rcon.Instance
            && rcon.Instance.Config.TryGetEntry<string>("3. Chat", "Server name", out var serverName))
        {
            name = serverName.Value;
        }

        if (RconPlayerName.Value != name)
            RconPlayerName.Value = name;
    }

    private void OnDestroy() => Config.Save();

    private void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(ConfigFileFullPath)) return;
        try
        {
            ModLogger.LogDebug(DebugMessages.ReadConfigCalled);
            Config.Reload();
        }
        catch
        {
            ModLogger.LogError(DebugMessages.ErrorLoadingConfig);
            ModLogger.LogError(DebugMessages.RequestCheckConfig);
        }
    }
}