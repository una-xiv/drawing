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

    private bool _mustReflow = true;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Reflow(Point? position = null)
    {
        if (IsDisposed || !ComputedStyle.IsVisible) return;
        if (!_mustReflow) return;

        Vector2 origin = new Vector2(position?.X ?? 0, position?.Y ?? 0);
        
        Layout.ComputeLayout(this, origin);
        _mustReflow = false;
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