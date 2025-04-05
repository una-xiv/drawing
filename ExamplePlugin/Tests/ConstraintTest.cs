using System.Linq;
using ImGuiNET;
using Una.Drawing;

namespace ExamplePlugin.Tests;

public class ConstraintTest : ITest
{
    public string Name => "Constraint";

    private Node? _node;

    public void OnActivate()
    {
        _node = Node.FromCode(
            """
            <style>
            #main {
                anchor: top-left;
                flow: horizontal;
                gap: 10;
                size: 0 0;
                auto-size: fit;
                padding: 10;
                background-color: 0x8000A000;
                background-gradient: radial 0x400000FF 0xFF000000;
                stroke-color: 0xFF0000A0;
                stroke-width: 2;
                border-radius: 8;
                border-color: 0x80A0DAFF;
                border-width: 2 0 0 0;
                border-inset: 2 2 0 2;
                
                .left {
                    auto-size: grow fit;
                    gap: 10;
                }
                .right {
                    auto-size: fit;
                    gap: 10;
                }
            }

            .button {
                max-width: 100;
                text-overflow: false;
                border-radius: 8;
                border-color: 0xFF494949;
                border-width: 2;
                background-color: 0xFF212021;
                background-gradient: vertical 0x20FFFFFF 0xA0000000;
                padding: 0 10;
                size: 0 32;
                text-align: middle-center;
                
                &:hover {
                    max-width: 150;
                    background-color: 0xFF414041;
                }
            }
            
            .icon {
                size: 32 32;
                icon-id: 111;
                image-contrast: 0.1;
                image-grayscale: true;
                image-rotation: 45.0;
                image-offset: 16 -16;
            }
            </style>
            <node id="main">
                <node class="left">
                    <node class="button" value="Button 1"/>
                    <node class="button" value="Button 2 with a long label"/>
                </node>
                <node class="right">
                    <node class="button" value="Button 3 also has a long label"/>
                    <node class="button" value="Button 4 yet again."/>
                    <node class="icon"/>
                </node>
            </node>
            """
        );

        foreach (var node in _node.QuerySelectorAll(".button"))
        {
            node.OnClick += n => DebugLogger.Log("Button clicked: " + n.NodeValue);
        }
    }

    public void Render()
    {
        _node?.Render(ImGui.GetBackgroundDrawList(), new(100, 100));
    }
}