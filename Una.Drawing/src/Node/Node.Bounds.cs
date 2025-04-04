/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// Defines the sizes and positions of the content and padding areas of this node.
    /// </summary>
    public NodeBounds Bounds { get; internal set; } = new();

    #region Box Model Properties

    /// <summary>
    /// A read-only property that represents the outer width of this node,
    /// based on the total width of the content and padding areas.
    /// </summary>
    public int OuterWidth => ComputedStyle.IsVisible ? Bounds.PaddingSize.Width : 0;

    /// <summary>
    /// A read-only property that represents the outer height of this node,
    /// based on the total height of the content and padding areas.
    /// </summary>
    public int OuterHeight => ComputedStyle.IsVisible ? Bounds.PaddingSize.Height : 0;

    /// <summary>
    /// A read-only property that represents the width of this node that is
    /// made up of the content area.
    /// </summary>
    public int Width => ComputedStyle.IsVisible ? Bounds.ContentSize.Width : 0;

    /// <summary>
    /// A read-only property that represents the height of this node that is
    /// made up of the content area.
    /// </summary>
    public int Height => ComputedStyle.IsVisible ? Bounds.ContentSize.Height : 0;

    #endregion
}
