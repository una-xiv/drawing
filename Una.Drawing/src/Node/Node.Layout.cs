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

    private bool     _mustReflow = true;
    private Vector2? _lastPosition;

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

        _lastPosition = position ?? Vector2.Zero;
        _mustReflow   = false;
        
        Layout.ComputeBounds(this);
        InvokeReflowHook();
        Layout.ComputeLayout(this, position ?? new Vector2());
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