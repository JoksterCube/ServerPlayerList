using BepInEx.Configuration;
using System.Runtime.Remoting.Messaging;

namespace JoksterCube.ServerPlayerList.Common;

internal class ConfigInfo<T>
{
    internal string Group { get; }
    internal string Name { get; }
    internal ConfigDescription Description { get; }
    internal T DefaultValue { get; }
    internal bool Synchronized { get; }

    internal ConfigInfo(string group, string name, string description, T defaultValue = default, bool synchronized = true) :
        this(group, name, new ConfigDescription(description), defaultValue, synchronized)
    { }

    internal ConfigInfo(string group, string name, ConfigDescription description, T defaultValue = default, bool synchronized = true)
    {
        Group = group;
        Name = name;
        Description = description;
        DefaultValue = defaultValue;
        Synchronized = synchronized;
    }
}
