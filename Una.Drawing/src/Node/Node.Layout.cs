/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public partial class Node
{
    internal Dictionary<Anchor.AnchorPoint, List<Node>> AnchorToChildNodes { get; private set; } = [];
    // internal Dictionary<Node, Anchor.AnchorPoint>       ChildNodeToAnchor { get; private set; } = [];

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