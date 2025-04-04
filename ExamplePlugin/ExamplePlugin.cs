using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ExamplePlugin.Tests;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin;

public sealed class ExamplePlugin : IDalamudPlugin
{
    [PluginService] private IChatGui _chatGui { get; set; } = null!;

    private readonly IDalamudPluginInterface   _plugin;
    private readonly Dictionary<string, ITest> _tests = [];

    private string _activeTest = "Script";

    public static Stylesheet GlobalStylesheet = new(
        [
            new(
                ".button",
                new() {
                    Size                    = new(0, 26),
                    Padding                 = new(0, 6),
                    BackgroundColor         = new(0xC01A1A1A),
                    BorderColor             = new(new(0xFF787A7A)),
                    BorderWidth             = new(1),
                    BorderRadius            = 5,
                    BorderInset             = new(2),
                    BackgroundGradient      = GradientColor.Vertical(new(0xC02F2A2A), null),
                    BackgroundGradientInset = new(5),
                    TextAlign               = Anchor.MiddleCenter,
                    TextOffset              = new(0, -1),
                    FontSize                = 10,
                }
            ),
            new(
                ".button:hover",
                new() {
                    Color                   = new(0xFF101010),
                    BackgroundColor         = new(0xC0EAEAEA),
                    BackgroundGradient      = GradientColor.Vertical(new(0xC02FFFFF), null),
                    BackgroundGradientInset = new(5),
                }
            ),
            new(
                ".button:active",
                new() { FontSize = 14, }
            )
        ]
    );

    public ExamplePlugin(IPluginLog logger, IDalamudPluginInterface plugin)
    {
        DrawingLib.Setup(plugin);

        DebugLogger.Writer = logger;
        _plugin            = plugin;

        Node.DrawDebugInfo               = false;
        Node.UseThreadedStyleComputation = true;

        var file = new FileInfo(
            Path.Combine(_plugin.DalamudAssetDirectory.FullName, "UIRes", "NotoSansKR-Regular.otf")
        );

        FontRegistry.SetNativeFontFamily(1, file);

        var tests = Assembly
                   .GetExecutingAssembly()
                   .GetTypes()
                   .Where(t => t.IsClass && t.IsAssignableTo(typeof(ITest)))
                   .ToList();

        foreach (var test in tests) {
            var instance = (ITest)Activator.CreateInstance(test)!;
            _tests[instance.Name] = instance;
        }
        
        _tests[_activeTest].OnActivate();

        _plugin.UiBuilder.Draw += OnDraw;
    }

    public void Dispose()
    {
        _plugin.UiBuilder.Draw -= OnDraw;

        DrawingLib.Dispose();
    }

    private void OnDraw()
    {
        ImGui.SetNextWindowSize(new(600, 110), ImGuiCond.Once);
        ImGui.SetNextWindowPos(new(10, 10), ImGuiCond.Once);
        ImGui.Begin("UnaDrawingTestSuite");

        if (ImGui.BeginCombo("Tests", _activeTest)) {
            if (ImGui.Selectable("None", _activeTest == "")) {
                _activeTest = "";
            }

            foreach (var test in _tests) {
                var selected = _activeTest == test.Key;

                if (ImGui.Selectable(test.Key, selected)) {
                    _activeTest = test.Key;
                    if (_tests.TryGetValue(_activeTest, out var t)) {
                        t.OnActivate();
                    }
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SetCursorPos(new(10, 56));

        var last = _tests.Keys.Last();

        foreach (var test in _tests) {
            if (ImGui.Button(test.Key)) {
                _activeTest = test.Key;
            }

            if (test.Key != last) {
                ImGui.SameLine();
            }
        }

        float scale = Node.ScaleFactor;

        if (ImGui.DragFloat("UI Scale", ref scale, 0.1f, 0.5f, 3.0f)) {
            Node.ScaleFactor = Math.Clamp(scale, 0.5f, 3.0f);
        }

        bool b = Node.ScaleAffectsBorders;

        if (ImGui.Checkbox("Affect borders", ref b)) {
            Node.ScaleAffectsBorders = b;
        }

        ImGui.SameLine();
        bool b2 = Node.DrawDebugInfo;

        if (ImGui.Checkbox("Draw debug bounds", ref b2)) {
            Node.DrawDebugInfo = b2;
        }

        ImGui.End();

        if (_tests.TryGetValue(_activeTest, out var activeTest)) {
            activeTest.Render();
        }
    }
}