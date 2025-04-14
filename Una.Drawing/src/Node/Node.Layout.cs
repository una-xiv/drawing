using System.Diagnostics;

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// <para>
    /// Invoked immediate after the bounding boxes of all nodes have been
    /// computed and immediately before the reflow process begins.
    /// </para>
    /// <para>
    /// Use this hook to perform any resize operations on the Bounds of the
    /// node, for example, manually resizing the node with certain constraints.
    /// </para>
    /// <remarks>
    /// This callback is only invoked if the node needs to reflow. This only
    /// happens if any property of this node or any of its dependencies has
    /// been modified that would affect its layout.
    /// </remarks>
    /// </summary>
    public ReflowDelegate? BeforeReflow;

    public delegate bool ReflowDelegate(Node node);

    internal Dictionary<Anchor.AnchorPoint, List<Node>> AnchorToChildNodes { get; } = [];

    internal double ReflowTime { get; private set; } = 0f;
    internal double LayoutTime { get; private set; } = 0f;
    internal double DrawTime   { get; private set; } = 0f;

    private bool      _mustReflow = true;
    private Vector2?  _lastPosition;
    private Stopwatch _metricStopwatch = new();
    private Rect      _previousBounds  = new(0, 0, 0, 0);

    /// <summary>
    /// Computes the bounding size of this node and all its descendants. This method
    /// is invoked automatically when the node is rendered when a reflow is required.
    /// You can call this method if you need to know the size of the node before it
    /// is being drawn.
    /// </summary>
    /// <remarks>
    /// Only the bounding sizes are computed. The bounding rects are not.
    /// </remarks>
    public void ComputeBoundingSize()
    {
        Layout.ComputeBounds(this);
    }

    public void Reflow(Vector2? position = null)
    {
        if (IsDisposed || !ComputedStyle.IsVisible) return;
        if (!_mustReflow && _lastPosition.Equals(position)) return;

        _mustReflow = false;

        _metricStopwatch.Restart();
        Layout.ComputeBounds(this);
        ReflowTime = _metricStopwatch.Elapsed.TotalMilliseconds;

        InvokeReflowHook();

        _lastPosition = position;
        _metricStopwatch.Restart();
        Layout.ComputeLayout(this, position ?? new Vector2());
        LayoutTime = _metricStopwatch.Elapsed.TotalMilliseconds;
    }

    private bool InvokeReflowHook()
    {
        if (IsDisposed) return false;

        var changed = false;

        lock (_childNodes) {
            foreach (Node child in _childNodes) {
                bool result         = child.InvokeReflowHook();
                if (result) changed = true;
            }
        }

        if (changed) Layout.ComputeBounds(this);

        return (BeforeReflow?.Invoke(this) ?? false) || changed;
    }

    private void ReassignAnchorNodes()
    {
        lock (AnchorToChildNodes) {
            lock (_childNodes) {
                AnchorToChildNodes.Clear();

                foreach (Node child in _childNodes) {
                    if (child.ComputedStyle.Anchor == Anchor.AnchorPoint.None) continue;

                    if (!AnchorToChildNodes.ContainsKey(child.ComputedStyle.Anchor.Point)) {
                        AnchorToChildNodes[child.ComputedStyle.Anchor.Point] = [];
                    }

                    AnchorToChildNodes[child.ComputedStyle.Anchor.Point].Add(child);
                }
            }
        }
    }
}