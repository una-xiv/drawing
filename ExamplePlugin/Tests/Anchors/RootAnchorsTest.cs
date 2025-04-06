using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Una.Drawing;

namespace ExamplePlugin.Tests.Anchors;

public class RootAnchorsTest : DrawingTest
{
    private readonly Dictionary<Anchor.AnchorPoint, Node> _nodes = [];

    /// <inheritdoc/>
    public override string Name => "Root Anchors";

    /// <inheritdoc/>
    public override string Category => "Anchors";

    /// <inheritdoc/>
    public override void OnActivate()
    {
        // Iterate over all anchor points and create a node for each one.
        foreach (var anchor in Enum.GetValues<Anchor.AnchorPoint>()) {
            Node node = CreateNodeFromFile("anchors.root.tpl");
            node.Style.Anchor = anchor;
            _nodes.Add(anchor, node);
        }
    }

    /// <inheritdoc/>
    public override void OnDeactivate()
    {
        foreach (var node in _nodes.Values) {
            node.Dispose();
        }

        _nodes.Clear();
    }

    /// <inheritdoc/>
    public override void RenderConfig()
    {
        ImGui.TextWrapped(
            "Renders nodes with different anchor points. Each anchor point can be seen as a red circle on which the node is anchored.");
    }

    /// <inheritdoc/>
    public override void RenderTest(ImDrawListPtr dl)
    {
        RenderAnchorAt(dl, Anchor.AnchorPoint.TopLeft, new(0, 0));
        RenderAnchorAt(dl, Anchor.AnchorPoint.TopCenter, new(200, 0));
        RenderAnchorAt(dl, Anchor.AnchorPoint.TopRight, new(400, 0));
        
        RenderAnchorAt(dl, Anchor.AnchorPoint.MiddleLeft, new(0, 200));
        RenderAnchorAt(dl, Anchor.AnchorPoint.MiddleCenter, new(200, 200));
        RenderAnchorAt(dl, Anchor.AnchorPoint.MiddleRight, new(400, 200));
        
        RenderAnchorAt(dl, Anchor.AnchorPoint.BottomLeft, new(0, 400));
        RenderAnchorAt(dl, Anchor.AnchorPoint.BottomCenter, new(200, 400));
        RenderAnchorAt(dl, Anchor.AnchorPoint.BottomRight, new(400, 400));
    }

    private void RenderAnchorAt(ImDrawListPtr dl, Anchor.AnchorPoint anchor, Vector2 pos)
    {
        Vector2 padding  = new(32, 32);
        Vector2 position = ImGui.GetWindowPos() + padding + pos;
        
        Node node = _nodes[anchor];

        node.NodeValue    = $"{anchor}";
        node.Style.Anchor = anchor;
        node.Render(dl, padding + pos);
        
        dl.AddCircleFilled(position, 8, 0xFF0000FF);
    }
}