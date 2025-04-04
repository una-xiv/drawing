using System;
using ImGuiNET;
using Una.Drawing;
using Una.Drawing.Script;

namespace ExamplePlugin.Tests;

public class CustomNode : Node
{
}

public class ScriptTest : ITest
{
    public string Name => "Script";

    private Node? _rootNode;

    public void OnActivate()
    {
        ScriptRegistry.Register("base",
            """
            <style>
            .menu {
                anchor: top-left;
                flow: vertical;
                gap: 8;
                padding: 10 5;
                border-width: 2;
                border-color: 0xFF00FF00;
                background-color: 0xa0f0c0c0;
                rounded-corners: all;
                border-radius: 10;
                shadow-size: 16;
                is-antialiased: false;
            }
            </style>
            """);

        ElementRegistry.Register<CustomNode>();

        const string code =
            """
                    <Style>
                    @import "base";
                    
                    .menu {
                        padding: 2 2;
                        background-color: 0x2AFF0000;
                    }
                    
                    .item {
                        auto-size: grow fit;
                        anchor: top-left;
                        padding: 4 8;
                        gap: 8;
                        
                        .icon {
                            anchor: middle-left;
                            size: 16;
                            background-color: 0xFF00FF00;
                            border-radius: 10;
                            rounded-corners: all;
                        }
                        
                        .label {
                            anchor: middle-left;
                            auto-size: grow fit;
                            font-size: 16;
                            text-align: middle-left;
                            size: 0 32;
                        }
                        
                        .alt {
                            anchor: middle-left;
                            font-size: 12;
                            text-align: middle-right;
                            size: 0 32;
                            padding: 0 0 0 32;
                        }
                        
                        &:hover {
                            .icon {
                                background-color: 0xFF00FFFF;
                                size: 24;
                            }
                            .label {
                                font-size: 20;
                            }
                        }
                    }
                    </Style>
                     
                    <Node Id="Test" class="menu">
                        <Node class="item">
                            <Node class="icon"/>
                            <Node class="label" value="This is a label" />
                            <Node class="alt" value="alt label 1" />
                        </Node>
                        <Node class="item">
                            <Node class="icon"/>
                            <Node class="label" value="This is another label" />
                            <Node class="alt" value="alt label 2" />
                        </Node>
                        <Node class="item">
                            <Node class="icon"/>
                            <Node class="label" value="This is yet another label" />
                            <Node class="alt" value="alt label 3" />
                        </Node>
                    </Node>
            """;

        try
        {
            var script = ScriptSource.FromCode(code);
            _rootNode = script.RootNode;

            foreach (var node in _rootNode!.QuerySelectorAll(".item"))
            {
                node.OnClick += n => DebugLogger.Log($"Clicked {n}");
            }

            DebugLogger.Log(_rootNode?.DumpTree() ?? "");
        }
        catch (Exception e)
        {
            DebugLogger.Error(e.Message);
            DebugLogger.Log(e.StackTrace ?? "");
        }
    }

    public void Render()
    {
        _rootNode?.Render(ImGui.GetBackgroundDrawList(), new(100, 100));
    }
}