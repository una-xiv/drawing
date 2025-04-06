using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ExamplePlugin.Tests;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using Una.Drawing;

namespace ExamplePlugin;

public sealed class ExamplePlugin : IDalamudPlugin
{
    private readonly Dictionary<string, DrawingTest> _tests = [];
    private readonly IDalamudPluginInterface         _plugin;
    private readonly ICommandManager                 _cmdManager;
    private readonly Stopwatch                       _stopwatch;
    private readonly Stopwatch                       _loopTimer;
    private readonly Stopwatch                       _testTimer;
    private readonly Stopwatch                       _peakTimer;

    private string _activeCategory = "";
    private string _activeTest     = "";

    private double        _lastFrameTimeMilliseconds;
    private Queue<double> _frameTimes;
    private double        _averageFrameTimeMilliseconds;
    private double        _averageIntervalMilliseconds = 500.0;
    private double        _elapsedAverageTimeMilliseconds;
    private double        _testTime;
    private double        _testPeakTime;
    private Exception?    _activateException;

    public ExamplePlugin(IPluginLog logger, IDalamudPluginInterface plugin, ICommandManager commandManager)
    {
        DrawingLib.Setup(plugin);

        DebugLogger.Writer = logger;
        _plugin            = plugin;
        _cmdManager        = commandManager;

        foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
            logger.Info($"Embedded file: {name}");
        }

        Node.DrawDebugInfo               = false;
        Node.UseThreadedStyleComputation = true;

        var file = new FileInfo(
            Path.Combine(_plugin.DalamudAssetDirectory.FullName, "UIRes", "NotoSansKR-Regular.otf")
        );

        FontRegistry.SetNativeFontFamily(1, file);

        var tests = Assembly
                   .GetExecutingAssembly()
                   .GetTypes()
                   .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(DrawingTest)))
                   .ToList();

        foreach (var test in tests) {
            var instance = (DrawingTest)Activator.CreateInstance(test)!;
            _tests[instance.Name] = instance;
        }

        _stopwatch                      = Stopwatch.StartNew();
        _loopTimer                      = Stopwatch.StartNew();
        _testTimer                      = Stopwatch.StartNew();
        _peakTimer                      = Stopwatch.StartNew();
        _lastFrameTimeMilliseconds      = 0.0;
        _frameTimes                     = new Queue<double>();
        _averageFrameTimeMilliseconds   = 0.0;
        _elapsedAverageTimeMilliseconds = 0.0;

        _plugin.UiBuilder.Draw += OnDraw;
        
        if (tests.Count > 0) {
            ActivateTest(tests.First().Name);
        }
    }

    public void Dispose()
    {
        _plugin.UiBuilder.Draw -= OnDraw;

        DrawingLib.Dispose();
    }

    private void OnDraw()
    {
        double deltaTime = _loopTimer.Elapsed.TotalMilliseconds;
        _loopTimer.Restart();

        if (_peakTimer.Elapsed.Seconds >= 2) {
            _peakTimer.Restart();
            _testPeakTime = 0;
        }

        ImGui.SetNextWindowSize(new(500, 300), ImGuiCond.Appearing);
        ImGui.SetNextWindowPos(new(ImGui.GetMainViewport().WorkPos.X + 32, ImGui.GetMainViewport().WorkPos.Y + 32),
            ImGuiCond.Appearing);
        ImGui.Begin("Una.Drawing Sandbox", ImGuiWindowFlags.AlwaysAutoResize);

        float scale = Node.ScaleFactor;
        if (ImGui.DragFloat("UI Scale", ref scale, 0.1f, 0.5f, 3.0f)) {
            Node.ScaleFactor = Math.Clamp(scale, 0.5f, 3.0f);
        }

        ImGui.SameLine();
        bool b = Node.ScaleAffectsBorders;
        if (ImGui.Checkbox("Affect borders", ref b)) {
            Node.ScaleAffectsBorders = b;
        }

        ImGui.SameLine();
        bool b2 = Node.DrawDebugInfo;
        if (ImGui.Checkbox("Draw debug bounds", ref b2)) {
            Node.DrawDebugInfo = b2;
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(" | ");

        ImGui.SameLine();
        if (ImGui.Button("Console")) {
            _cmdManager.ProcessCommand("/xllog");
        }

        ImGui.SameLine();
        if (ImGui.Button("Stats")) {
            _cmdManager.ProcessCommand("/xlstats");
        }

        // Render tabs for each test
        ImGui.BeginTabBar("Categories");
        foreach (var category in GetTestCategories()) {
            if (ImGui.BeginTabItem(category)) {
                _activeCategory = category;
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
        ImGui.BeginChild("Tests", new Vector2(0, 300), true, ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.BeginTabBar("Tests_Tabs");
        foreach (var test in GetTestsFromCategory(_activeCategory)) {
            if (ImGui.BeginTabItem(test.Name)) {
                ActivateTest(test.Name);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        DrawingTest? activeTest;

        if (_tests.TryGetValue(_activeTest, out activeTest)) {
            try {
                activeTest.RenderConfig();
            } catch (Exception ex) {
                ImGui.TextUnformatted($"Error: {ex.Message}");
                ImGui.Text(ex.StackTrace);
            }
        }

        ImGui.EndChild();

        double frameTime = UpdateFrameTime(deltaTime);

        string fpsText       = $"FPS: {Math.Round(1.0 / (_averageFrameTimeMilliseconds / 1000.0), 2)}";
        string frameTimeText = $"{frameTime:F2} ms";
        string testTimeText  = $"Draw time: {_testTime:F2} ms\tPeak: {_testPeakTime:F2} ms";

        ImGui.TextUnformatted($"{fpsText}\t{frameTimeText}");
        ImGui.SameLine();

        float testTimeWidth = ImGui.CalcTextSize(testTimeText).X;
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - testTimeWidth - 10f);
        ImGui.TextUnformatted($"{testTimeText}");
        ImGui.End();

        if (activeTest == null) return;

        Vector2 vpSize       = ImGui.GetMainViewport().WorkSize;
        Vector2 vpPos        = ImGui.GetMainViewport().WorkPos;
        Vector2 testAreaSize = new(1280, 900);
        Vector2 testAreaPos  = new(vpPos.X + 32, vpPos.Y + (vpSize.Y - testAreaSize.Y - 32));
        Vector2 titleSize    = new(1280, 24);
        Vector2 titlePos     = testAreaPos - titleSize with { X = 0 };

        ImDrawListPtr dl = ImGui.GetBackgroundDrawList();

        string testTitle = $"{activeTest.Category} / {activeTest.Name}  -  {activeTest.GetType().FullName}";

        dl.AddRectFilled(titlePos, titlePos + titleSize, 0xF0191A19, 0);
        dl.AddText(titlePos + new Vector2(11, 2), 0xFF000000, testTitle);
        dl.AddText(titlePos + new Vector2(11, 5), 0xFF000000, testTitle);
        dl.AddText(titlePos + new Vector2(9, 2), 0xFF000000, testTitle);
        dl.AddText(titlePos + new Vector2(9, 5), 0xFF000000, testTitle);
        dl.AddText(titlePos + new Vector2(10, 3), 0xFFFFFFFF, testTitle);

        ImGui.SetNextWindowPos(testAreaPos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(testAreaSize, ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.Begin("TestFrame", TestFrameWindowFlags);
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize(),
            activeTest.BackgroundColor, 0);

        try {
            _testTimer.Restart();

            if (_activateException != null) {
                RenderException(_activateException);
            } else {
                activeTest.RenderTest(ImGui.GetWindowDrawList());
            }
        } catch (Exception ex) {
            RenderException(ex);
        } finally {
            _testTime     = _loopTimer.Elapsed.TotalMilliseconds;
            _testPeakTime = Math.Max(_testTime, _testPeakTime);
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private void ActivateTest(string name)
    {
        if (_activeTest == name) return;
        if (_tests.TryGetValue(_activeTest, out var previousTest)) {
            previousTest.OnDeactivate();
        }

        _activeTest        = name;
        _activateException = null;
        _testPeakTime      = 0;
        
        _peakTimer.Restart();

        try {
            if (_tests.TryGetValue(_activeTest, out var test)) {
                test.OnActivate();
            }
        } catch (Exception ex) {
            _activateException = ex;
        }
    }

    private List<string> GetTestCategories()
    {
        List<string> result = [];

        foreach (var test in _tests
                            .Where(test => !string.IsNullOrEmpty(test.Value.Category))
                            .Where(test => !result.Contains(test.Value.Category))
        ) {
            result.Add(test.Value.Category);
        }

        result.Sort();

        return result;
    }

    private List<DrawingTest> GetTestsFromCategory(string category)
    {
        return _tests.Where(test => test.Value.Category == category)
                     .Select(test => test.Value)
                     .OrderBy(test => test.Name)
                     .ToList();
    }

    public double UpdateFrameTime(double deltaTime) //deltaTime is the time since the last update
    {
        long elapsedTicks = _stopwatch.ElapsedTicks;
        _stopwatch.Restart();

        _lastFrameTimeMilliseconds = (elapsedTicks * 1000.0) / Stopwatch.Frequency;

        _frameTimes.Enqueue(_lastFrameTimeMilliseconds);
        _elapsedAverageTimeMilliseconds += deltaTime;

        while (_elapsedAverageTimeMilliseconds >= _averageIntervalMilliseconds) {
            double sum = 0;
            foreach (double frameTime in _frameTimes) {
                sum += frameTime;
            }

            _averageFrameTimeMilliseconds = sum / _frameTimes.Count;
            _frameTimes.Clear();
            _elapsedAverageTimeMilliseconds -= _averageIntervalMilliseconds;
        }

        return _lastFrameTimeMilliseconds;
    }

    private static void RenderException(Exception ex)
    {
        ImGui.SetCursorPos(new(10, 10));
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
        ImGui.TextUnformatted($"Error: {ex.Message}\n");
        ImGui.PopStyleColor();
        ImGui.SetCursorPosX(16);
        ImGui.TextUnformatted(ex.StackTrace);
    }

    private static ImGuiWindowFlags TestFrameWindowFlags =>
        ImGuiWindowFlags.None
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoDecoration
        | ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoInputs
        | ImGuiWindowFlags.NoMouseInputs
        | ImGuiWindowFlags.NoMove
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoBackground;
}