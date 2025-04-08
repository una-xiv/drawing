using ImGuiNET;

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// <para>
    /// Enables or disables sorting behavior for this node.
    /// </para>
    /// <para>
    /// This automatically makes all immediate children interactive and allows
    /// the user to sort the child nodes by dragging them around. The flow
    /// direction of the node will be used to determine the sorting direction.
    /// </para>
    /// </summary>
    public bool Sortable {
        get => _isSortingEnabled;
        set {
            _isSortingEnabled = value;

            if (value) {
                EnableSorting();
            } else {
                DisableSorting();
            }
        }
    }

    private bool  _isSortingEnabled;
    private Node? _sortableHoveredNode;
    private Node? _sortableDraggedNode;
    private int   _sortableDraggedNodeIndex;

    private void EnableSorting()
    {
        OnChildAdded   += OnSortableChildAdded;
        OnChildRemoved += OnSortableChildRemoved;

        lock (_childNodes) {
            foreach (Node child in _childNodes) {
                OnSortableChildAdded(child);
            }
        }
    }

    private void DisableSorting()
    {
        OnChildAdded   -= OnSortableChildAdded;
        OnChildRemoved -= OnSortableChildRemoved;

        lock (_childNodes) {
            foreach (Node child in _childNodes) {
                OnSortableChildRemoved(child);
            }
        }
    }

    private void OnSortableChildAdded(Node child)
    {
        // Every child node must have a valid SortIndex.
        lock (_childNodes) {
            if (child.SortIndex == 0) {
                child.SortIndex = _childNodes.Count - 1; // SortIndex is 0-based.
            }
        }

        child.OnDragStart += OnSortableChildDragStart;
        child.OnDragMove  += OnSortableChildDragMove;
        child.OnDragEnd   += OnSortableChildDragEnd;
    }

    private void OnSortableChildRemoved(Node child)
    {
        // Re-apply sort indices on all children to fill the gap.
        lock (_childNodes) {
            for (int i = 0; i < _childNodes.Count; i++) {
                _childNodes[i].SortIndex = i;
            }
        }

        child.OnDragStart -= OnSortableChildDragStart;
        child.OnDragMove  -= OnSortableChildDragMove;
        child.OnDragEnd   -= OnSortableChildDragEnd;
    }

    private void OnSortableChildDragStart(Node child)
    {
        _sortableDraggedNode = child;
    }

    private void OnSortableChildDragMove(Node child)
    {
        if (!IsMouseInNodeBounds()) return;

        _sortableHoveredNode = FindNodeAtMousePos();
        if (null == _sortableHoveredNode || _sortableHoveredNode == child) {
            return;
        }

        Vector2 startPos;
        Vector2 endPos;
        bool    isAbove;

        if (ComputedStyle.Flow == Flow.Vertical) {
            isAbove = ImGui.GetMousePos().Y < _sortableHoveredNode.Bounds.MarginRect.Center.Y;
            startPos = isAbove
                ? _sortableHoveredNode.Bounds.MarginRect.TopLeft
                : _sortableHoveredNode.Bounds.MarginRect.BottomLeft;
            endPos = isAbove
                ? _sortableHoveredNode.Bounds.MarginRect.TopRight
                : _sortableHoveredNode.Bounds.MarginRect.BottomRight;

            // If we have a gap between the nodes, we need to adjust the start and end positions.
            if (ComputedStyle.Gap > 0) {
                startPos.Y += (isAbove ? -ComputedStyle.Gap : ComputedStyle.Gap) / 2f;
                endPos.Y   =  startPos.Y;
            }
        } else {
            isAbove = ImGui.GetMousePos().X < _sortableHoveredNode.Bounds.MarginRect.Center.X;

            startPos = isAbove
                ? _sortableHoveredNode.Bounds.MarginRect.TopLeft
                : _sortableHoveredNode.Bounds.MarginRect.TopRight;
            endPos = isAbove
                ? _sortableHoveredNode.Bounds.MarginRect.BottomLeft
                : _sortableHoveredNode.Bounds.MarginRect.BottomRight;

            // If we have a gap between the nodes, we need to adjust the start and end positions.
            if (ComputedStyle.Gap > 0) {
                startPos.X += (isAbove ? -ComputedStyle.Gap : ComputedStyle.Gap) / 2f;
                endPos.X   =  startPos.X;
            }
        }

        // Draw a line between the two nodes to indicate the swap.
        ImGui.GetForegroundDrawList().AddLine(
            startPos, endPos,
            ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)),
            2f
        );

        // Find the new sort index for the dragged node.
        _sortableDraggedNodeIndex = _sortableHoveredNode.SortIndex + (isAbove ? 0 : 1);
    }

    private void OnSortableChildDragEnd(Node child)
    {
        if (_sortableDraggedNode == null || _sortableHoveredNode == null || _sortableDraggedNodeIndex < 0) {
            _sortableDraggedNode      = null;
            _sortableHoveredNode      = null;
            _sortableDraggedNodeIndex = -1;
            return;
        }

        int  calculatedTargetSlot = _sortableDraggedNodeIndex;
        Node hoveredNode          = _sortableHoveredNode;
        Node draggedNode          = _sortableDraggedNode;

        lock (_childNodes) {
            int draggedIndex = _childNodes.IndexOf(draggedNode);
            if (draggedIndex == -1) return;

            int finalTargetIndex = calculatedTargetSlot;

            bool droppedOnTopHalf = (calculatedTargetSlot == hoveredNode.SortIndex);
            if (droppedOnTopHalf && hoveredNode.SortIndex == draggedIndex + 1) {
                finalTargetIndex = draggedIndex;
            }

            bool droppedOnBottomHalf = (calculatedTargetSlot == hoveredNode.SortIndex + 1);
            if (droppedOnBottomHalf && hoveredNode.SortIndex == draggedIndex - 1) {
                finalTargetIndex = draggedIndex;
            }

            if (finalTargetIndex == calculatedTargetSlot && droppedOnBottomHalf &&
                draggedIndex < hoveredNode.SortIndex) {
                finalTargetIndex = hoveredNode.SortIndex;
            }

            int targetIndex = finalTargetIndex;

            if (draggedIndex == targetIndex) {
                _sortableDraggedNode      = null;
                _sortableHoveredNode      = null;
                _sortableDraggedNodeIndex = -1;
                return;
            }

            Dictionary<Node, int> newSortIndex = [];

            for (int i = 0; i < _childNodes.Count; i++) {
                Node currentNode = _childNodes[i];

                if (currentNode == draggedNode) {
                    newSortIndex[currentNode] = targetIndex;
                    continue;
                }

                if (draggedIndex < targetIndex) {
                    newSortIndex[currentNode] = i > draggedIndex && i <= targetIndex ? i - 1 : 1;
                    continue;
                }

                if (i >= targetIndex && i < draggedIndex) {
                    newSortIndex[currentNode] = i + 1;
                    continue;
                }

                newSortIndex[currentNode] = i;
            }

            foreach (var (node, index) in newSortIndex) {
                if (node.SortIndex != index) {
                    node.SortIndex = index;
                }
            }
        }

        _sortableDraggedNode      = null;
        _sortableHoveredNode      = null;
        _sortableDraggedNodeIndex = -1;
    }

    private bool IsMouseInNodeBounds(EdgeSize expand = default)
    {
        Rect rect = Bounds.MarginRect.Copy();
        rect.Expand(expand);
        
        return rect.Contains(ImGui.GetMousePos());
    }

    private Node? FindNodeAtMousePos()
    {
        float    g      = ComputedStyle.Gap;
        float    ph     = ComputedStyle.Padding.HorizontalSize;
        float    pv     = ComputedStyle.Padding.VerticalSize;
        EdgeSize expand = ComputedStyle.Flow == Flow.Vertical ? new(g, ph) : new(pv, g);

        lock (_childNodes) {
            foreach (Node node in _childNodes) {
                if (_sortableDraggedNode != null && node == _sortableDraggedNode) {
                    continue;
                }

                if (node.IsMouseInNodeBounds(expand)) {
                    return node;
                }
            }
        }

        return null;
    }
}