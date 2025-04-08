using ImGuiNET;
using System;
using System.Numerics;
using System.Reflection;
using Una.Drawing;

namespace ExamplePlugin.Tests;

internal abstract class SimpleUdtTest : DrawingTest
{
    protected abstract string UdtFileName { get; }

    protected UdtDocument? Document;

    private readonly string[] _tabs     = ["Test", "Node Tree", "Stylesheet", "XML Source"];
    private readonly string[] _nodeTabs = ["Node Info", "Computed Style"];

    private string _activeTab     = "Node Tree";
    private string _activeNodeTab = "Node Info";

    /// <summary>
    /// Invoked when the test is rendered.
    /// </summary>
    /// <remarks>
    /// Note that any time spent in this method is added to the test's draw time.
    /// </remarks>
    protected virtual void OnRenderTest(ImDrawListPtr dl) { }

    /// <summary>
    /// Render the configuration UI for this test. ImGui can be used here to render
    /// elements in the configuration window.
    /// </summary>
    protected virtual void OnRenderConfig() { }

    #region DrawingTest Implementation

    public sealed override void OnActivate()
    {
        Document      = UdtLoader.LoadFromAssembly(Assembly.GetExecutingAssembly(), UdtFileName);
        _selectedNode = Document?.RootNode;
    }

    public sealed override void OnDeactivate()
    {
        Document?.RootNode?.Dispose();
    }

    public sealed override void RenderConfig()
    {
        ImGui.BeginTabBar("TestTabs", ImGuiTabBarFlags.FittingPolicyScroll);

        foreach (var tab in _tabs) {
            if (ImGui.BeginTabItem(tab)) {
                _activeTab = tab;
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        switch (_activeTab) {
            case "Test":
                OnRenderConfig();
                RenderRootNodeResizeConfig();
                break;
            case "Node Tree":
                RenderNodeTreeViewer();
                break;
            case "Stylesheet":
                ImGui.TextWrapped(Document?.Stylesheet?.ToString() ?? "No stylesheet found in UDT.");
                break;
        }
    }

    public sealed override void RenderTest(ImDrawListPtr dl)
    {
        OnRenderTest(dl);
        Document?.RootNode?.Render(dl, new(10, 10));

        if (_hoveredNode != null) {
            Rect outer   = _hoveredNode.Bounds.MarginRect;
            Rect inner   = _hoveredNode.Bounds.PaddingRect;
            Rect content = _hoveredNode.Bounds.ContentRect;

            dl.AddRect(outer.TopLeft, outer.BottomRight, 0xFF0000FF, 0, ImDrawFlags.None, 1);
            dl.AddRect(inner.TopLeft, inner.BottomRight, 0xFFFFCC00, 0, ImDrawFlags.None, 1);
            dl.AddRect(content.TopLeft, content.BottomRight, 0xFF00FF00, 0, ImDrawFlags.None, 1);
        }

        if (_selectedNode != null) {
            Rect outer = _selectedNode.Bounds.MarginRect;
            dl.AddRect(outer.TopLeft, outer.BottomRight, 0xFF00FF00, 0, ImDrawFlags.None, 1);
        }

        _hoveredNode = null;
    }

    #endregion

    #region Root Node Resize Config

    private void RenderRootNodeResizeConfig()
    {
        if (null == Document?.RootNode) return;

        Node  rootNode    = Document.RootNode;
        Size  paddingSize = rootNode.Bounds.PaddingSize;
        float width       = paddingSize.Width;
        float height      = paddingSize.Height;

        ImGui.Separator();
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Root Node Size");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        if (ImGui.DragFloat("##rootWidth", ref width, 1f, 0, 1000)) {
            rootNode.Style.Size = new(width, height);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        if (ImGui.DragFloat("##rootHeight", ref height, 1f, 0, 1000)) {
            rootNode.Style.Size = new(width, height);
        }
    }

    #endregion

    #region Node Tree

    private int   _nodeTreeId;
    private Node? _selectedNode;
    private Node? _hoveredNode;

    private void RenderNodeTreeViewer()
    {
        if (Document?.RootNode is null) return;

        float height = MathF.Max(ImGui.GetWindowHeight() - 700, 300);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.BeginChild("NodeTreeViewer", new(0, height), true,
            ImGuiWindowFlags.HorizontalScrollbar);
        _nodeTreeId = 0;
        RenderNodeTreeViewerNode(Document.RootNode);
        ImGui.EndChild();
        ImGui.PopStyleVar();

        ImGui.BeginChild("NodeTreeViewerDetails", new(0, -1), false);

        ImGui.Dummy(new Vector2(0, 0));
        
        ImGui.BeginTabBar("NodeDetailsTabs", ImGuiTabBarFlags.FittingPolicyScroll);
        foreach (var tab in _nodeTabs) {
            if (ImGui.BeginTabItem(tab)) {
                _activeNodeTab = tab;
                ImGui.EndTabItem();
            }
        }
        ImGui.EndTabBar();
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.BeginChild("NodeDetailsTab", new(0, -1), true, ImGuiWindowFlags.AlwaysUseWindowPadding);
        
        switch (_activeNodeTab) {
            case "Node Info":
                RenderNodeInfoTab();
                return;
            case "Computed Style":
                RenderComputedStyleTab();
                return;
        }
        
        ImGui.EndChild();
        ImGui.PopStyleVar();
        ImGui.EndChild();
    }

    private void RenderNodeInfoTab()
    {
        if (_selectedNode == null) return;
        
        ImGui.BeginTable("NodeInfoTable", 2, ImGuiTableFlags.Borders);
        
        ImGui.TableSetupColumn("Property");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
    
        foreach (var property in _selectedNode.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
            var actualType = property.PropertyType;
            if (actualType.IsGenericType && actualType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                actualType = Nullable.GetUnderlyingType(actualType)!;
            }
            
            if (actualType == typeof(int) || actualType == typeof(float) || actualType == typeof(double) || 
                actualType == typeof(decimal) || actualType == typeof(string) || actualType == typeof(bool) || 
                actualType == typeof(string) || actualType == typeof(ObservableHashSet<string>)) {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(property.Name);
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(property.GetValue(_selectedNode)?.ToString() ?? "null");
            }
        }
        
        ImGui.EndTable();
    }

    private void RenderComputedStyleTab()
    {
        if (_selectedNode == null) return;
        
        ImGui.BeginTable("ComputedStyleTable", 2, ImGuiTableFlags.Borders);
        
        ImGui.TableSetupColumn("Property");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
        
        foreach (var property in typeof(ComputedStyle).GetFields(BindingFlags.Instance | BindingFlags.Public)) {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(property.Name);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(property.GetValue(_selectedNode.ComputedStyle)?.ToString() ?? "null");
        }
    
        foreach (var property in typeof(ComputedStyle).GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(property.Name);
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(property.GetValue(_selectedNode.ComputedStyle)?.ToString() ?? "null");
        }
        
        ImGui.EndTable();
    }
    
    private void RenderNodeTreeViewerNode(Node node)
    {
        _nodeTreeId++;
        ImGui.PushID(_nodeTreeId);

        if (node.ChildNodes.Count == 0) {
            if (ImGui.TreeNodeEx(node.ToString(),
                ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.Bullet |
                (_selectedNode == node ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None))) {
                if (ImGui.IsItemClicked()) {
                    _selectedNode = node == _selectedNode ? null : node;
                }

                if (ImGui.IsItemHovered()) {
                    _hoveredNode = node;
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
            return;
        }

        if (ImGui.TreeNodeEx(node.ToString(),
            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanFullWidth |
            (_selectedNode == node ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None))) {
            if (ImGui.IsItemClicked()) {
                _selectedNode = node == _selectedNode ? null : node;
            }

            if (ImGui.IsItemHovered()) {
                _hoveredNode = node;
            }

            foreach (var child in node.ChildNodes) {
                RenderNodeTreeViewerNode(child);
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    #endregion
}