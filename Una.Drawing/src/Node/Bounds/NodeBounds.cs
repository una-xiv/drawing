/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public class NodeBounds
{
    /// <summary>
    /// Represents the size of the border area of this node, which is the area
    /// that surrounds the content and padding areas of the node.
    /// </summary>
    public Size PaddingSize { get; set; } = new(0, 0);

    /// <summary>
    /// Represents the size of the content area of this node, which is the area
    /// that contains the bounding box areas of its children or the content of
    /// the node itself, whichever is larger.
    /// </summary>
    public Size ContentSize  { get; set; } = new(0, 0);

    /// <summary>
    /// Represents the border area of this node, which is the area that surrounds
    /// the content and padding areas of the node.
    /// </summary>
    public Rect PaddingRect { get; internal set; } = new(0, 0, 0, 0);

    /// <summary>
    /// Represents the content area of this node, which is the area that contains
    /// the bounding box areas of its children or the content of the node itself,
    /// whichever is larger.
    /// </summary>
    public Rect ContentRect { get; internal set; } = new(0, 0, 0, 0);
}
