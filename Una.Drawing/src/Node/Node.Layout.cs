namespace Una.Drawing;

public partial class Node
{
    internal Dictionary<Anchor.AnchorPoint, List<Node>> AnchorToChildNodes { get; } = [];

    private bool     _mustReflow = true;
    private Vector2? _lastPosition;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Reflow(Vector2? position = null)
    {
        if (IsDisposed || !ComputedStyle.IsVisible) return;
        if (!_mustReflow && _lastPosition.Equals(position)) return;

        _lastPosition = position ?? Vector2.Zero;
        _mustReflow   = false;
        
        Layout.ComputeLayout(this, position ?? new Vector2());
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