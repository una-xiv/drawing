using Dalamud.Configuration;
using System;

namespace ExamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public string? LastTestName { get; set; } = string.Empty;
}