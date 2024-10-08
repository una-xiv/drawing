﻿/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System.Linq;

namespace Una.Drawing;

public partial class Node
{
    public event Action<Node>? OnClick;
    public event Action<Node>? OnMiddleClick;
    public event Action<Node>? OnRightClick;
    public event Action<Node>? OnMouseEnter;
    public event Action<Node>? OnMouseLeave;
    public event Action<Node>? OnMouseDown;
    public event Action<Node>? OnMouseUp;
    public event Action<Node>? OnDelayedMouseEnter;

    /// <summary>
    /// True if the element has any interactive event listeners attached to it.
    /// </summary>
    public bool IsInteractive =>
        !IsDisabled && (
            null != Tooltip
            || null != OnClick
            || null != OnMiddleClick
            || null != OnRightClick
            || null != OnMouseEnter
            || null != OnDelayedMouseEnter
            || null != OnMouseLeave
            || null != OnMouseDown
            || null != OnMouseUp
        );

    /// <summary>
    /// Whether the element should have the "hover" tag added when the mouse is over it.
    /// </summary>
    public bool EnableHoverTag { get; set; } = true;

    /// <summary>
    /// True if the element has any primary interaction event listeners attached to it.
    /// </summary>
    public bool HasPrimaryInteraction => !IsDisabled && (null != OnClick || null != OnMouseUp || null != OnMouseDown);

    /// <summary>
    /// Set to true from an event listener to stop remaining event listeners from being called.
    /// </summary>
    public bool CancelEvent { get; set; }

    /// <summary>
    /// True if the mouse cursor is currently inside the bounding box of the element.
    /// </summary>
    public bool IsMouseOver { get; private set; }

    /// <summary>
    /// True if one of the mouse buttons in held down while the cursor is over the element.
    /// </summary>
    public bool IsMouseDown { get; private set; }

    /// <summary>
    /// True if one of the mouse buttons in held down while the cursor is over a different element.
    /// </summary>
    public bool IsMouseDownOverOtherNode { get; private set; }

    public bool IsMiddleMouseDown { get; private set; }
    public bool IsRightMouseDown { get; private set; }

    /// <summary>
    /// True if this element currently has focus.
    /// </summary>
    public bool IsFocused { get; private set; }

    /// <summary>
    /// True if this node cannot be interacted with, regardless of bound event
    /// listeners.
    /// </summary>
    public bool IsDisabled { get; set; }

    private bool _isInWindowOrInteractiveParent;
    private bool _didStartInteractive;
    private bool _didStartDelayedMouseEnter;
    private long _mouseOverStartTime;
    private long _isVisibleSince;

    private void SetupInteractive(ImDrawListPtr drawList)
    {
        _didStartInteractive = false;

        switch (IsDisabled) {
            case true when !_tagsList.Contains("disabled"):
                _tagsList.Add("disabled");
                break;
            case false when _tagsList.Contains("disabled"):
                _tagsList.Remove("disabled");
                break;
        }

        if (IsDisabled || !IsInteractive || !IsVisible) {
            MouseCursor.RemoveMouseOver(this);
            return;
        }

        if (_isVisibleSince == 0) _isVisibleSince = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        // Debounce the interactive state to prevent unintentional clicks
        // when toggling visibility on elements on the same position.
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _isVisibleSince < 100) return;

        _didStartInteractive = true;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize,    Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,    Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,     Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,  0);

        string  imGuiId           = InternalId;
        Vector2 boundingBoxSize   = Bounds.PaddingSize.ToVector2();
        Node?   interactiveParent = GetInteractiveParent();
        _isInWindowOrInteractiveParent = IsInWindowDrawList(drawList) || interactiveParent != null;

        // Disabled to allow for multi-monitor support. Leaving this here in case something breaks.
        // ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

        if (_isInWindowOrInteractiveParent) {
            ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
            ImGui.BeginChild(imGuiId, boundingBoxSize, false, InteractiveWindowFlags);
            ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
        } else {
            ImGui.SetNextWindowPos(Bounds.PaddingRect.TopLeft, ImGuiCond.Always);
            ImGui.SetNextWindowSize(boundingBoxSize, ImGuiCond.Always);
            ImGui.Begin(imGuiId, InteractiveWindowFlags);
        }

        bool wasHovered = IsMouseOver;

        ImGui.SetCursorScreenPos(Bounds.PaddingRect.TopLeft);
        ImGui.InvisibleButton($"{imGuiId}##Button", Bounds.PaddingSize.ToVector2());
        IsMouseOver = ImGui.IsItemHovered();
        IsFocused   = ImGui.IsItemFocused();

        if (IsMouseOver && HasPrimaryInteraction && EnableHoverTag) {
            MouseCursor.RegisterMouseOver(this);
        } else {
            MouseCursor.RemoveMouseOver(this);
        }

        switch (IsMouseOver && HasPrimaryInteraction && EnableHoverTag) {
            case true when !_tagsList.Contains("hover"):
                _tagsList.Add("hover");
                break;
            case false when _tagsList.Contains("hover"):
                _tagsList.Remove("hover");
                break;
        }

        switch (IsFocused) {
            case true when !_tagsList.Contains("focus"):
                _tagsList.Add("focus");
                break;
            case false when _tagsList.Contains("focus"):
                _tagsList.Remove("focus");
                break;
        }

        switch (IsMouseDown) {
            case true when !_tagsList.Contains("active"):
                _tagsList.Add("active");
                break;
            case false when _tagsList.Contains("active"):
                _tagsList.Remove("active");
                break;
        }

        if (Tooltip != null && IsMouseOver && _mouseOverStartTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - 500) {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,  new Vector2(8, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 6);
            ImGui.PushStyleColor(ImGuiCol.Border,   0xFF3F3F3F);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF252525);
            ImGui.PushStyleColor(ImGuiCol.Text,     0xFFCACACA);
            ImGui.BeginTooltip();
            ImGui.SetCursorPos(new(8, 4));
            ImGui.TextUnformatted(Tooltip);
            ImGui.EndTooltip();
            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(2);
        }

        switch (wasHovered) {
            case false when IsMouseOver:
                RaiseEvent(OnMouseEnter);
                _mouseOverStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                break;
            case true when !IsMouseOver:
                RaiseEvent(OnMouseLeave);
                _didStartDelayedMouseEnter = false;
                IsMouseDown                = false;
                IsMiddleMouseDown          = false;
                IsRightMouseDown           = false;
                break;
        }

        if (IsMouseOver) {
            if (HasPrimaryInteraction) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            if (_mouseOverStartTime < DateTimeOffset.Now.ToUnixTimeMilliseconds() - 50) {
                if (!_didStartDelayedMouseEnter) {
                    RaiseEvent(OnDelayedMouseEnter);
                    _didStartDelayedMouseEnter = true;
                }
            }

            if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) {
                if (!IsMouseDown) {
                    RaiseEvent(OnMouseDown);
                    IsMouseDown = true;
                }
            } else if (ImGui.IsMouseDown(ImGuiMouseButton.Middle)) {
                if (!IsMiddleMouseDown) {
                    IsMiddleMouseDown = true;
                }
            } else if (ImGui.IsMouseDown(ImGuiMouseButton.Right)) {
                if (!IsRightMouseDown) {
                    IsRightMouseDown = true;
                }
            } else {
                if (IsMouseDown && !IsMouseDownOverOtherNode) {
                    RaiseEvent(OnMouseUp);
                    RaiseEvent(OnClick);
                    IsMouseDown = false;
                }
                if (IsMiddleMouseDown) {
                    RaiseEvent(OnMiddleClick);
                    IsMiddleMouseDown = false;
                }
                if (IsRightMouseDown) {
                    RaiseEvent(OnRightClick);
                    IsRightMouseDown = false;
                }
                if (IsMouseDownOverOtherNode) {
                    RaiseEvent(OnMouseUp);
                }
            }
        } else {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) {
                IsMouseDownOverOtherNode = true;
            }
        }

        if (IsMouseDownOverOtherNode && !ImGui.IsMouseDown(ImGuiMouseButton.Left)) {
            IsMouseDownOverOtherNode = false;
        }
    }

    private void EndInteractive()
    {
        if (!_didStartInteractive) return;

        if (_isInWindowOrInteractiveParent) {
            ImGui.EndChild();
        } else {
            ImGui.End();
        }

        ImGui.PopStyleVar(5);
    }

    /// <summary>
    /// Returns the top-most interactive parent element.
    /// </summary>
    /// <returns></returns>
    private Node? GetInteractiveParent()
    {
        Node? immediateParent   = ParentNode;
        Node? interactiveParent = null;

        while (immediateParent != null) {
            if (immediateParent.IsInteractive) interactiveParent = immediateParent;
            immediateParent = immediateParent.ParentNode;
        }

        return interactiveParent;
    }

    /// <summary>
    /// Returns true if the given drawList is not the foreground or background drawList.
    /// </summary>
    private static unsafe bool IsInWindowDrawList(ImDrawListPtr drawList)
    {
        return
            drawList.NativePtr != ImGui.GetForegroundDrawList().NativePtr
            && drawList.NativePtr != ImGui.GetBackgroundDrawList().NativePtr;
    }

    private void RaiseEvent(Action<Node>? action)
    {
        if (action == null) return;

        CancelEvent = false;
        foreach (var handler in action.GetInvocationList().Reverse()) {
            if (!CancelEvent) handler.DynamicInvoke(this);
            if (CancelEvent) break;
        }
        CancelEvent = false;
    }

    private void DisposeEventHandlersOf<T>(Action<T>? action)
    {
        if (action == null) return;

        foreach (Delegate handler in action.GetInvocationList()) {
            if (handler is Action<T> del) action -= del;
        }
    }

    private void DisposeEventHandlersOf<T, K>(Action<T, K>? action)
    {
        if (action == null) return;

        foreach (Delegate handler in action.GetInvocationList()) {
            if (handler is Action<T, K> del) action -= del;
        }
    }

    private void DisposeEventHandlersOf(Action? action)
    {
        if (action == null) return;

        foreach (Delegate handler in action.GetInvocationList()) {
            if (handler is Action del) action -= del;
        }
    }

    private static ImGuiWindowFlags InteractiveWindowFlags =>
        ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoBackground
        | ImGuiWindowFlags.NoDecoration
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoScrollWithMouse
        | ImGuiWindowFlags.NoCollapse
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoDocking
        | ImGuiWindowFlags.NoNavFocus
        | ImGuiWindowFlags.NoNavInputs
        | ImGuiWindowFlags.NoFocusOnAppearing;
}