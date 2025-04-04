/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

/// <summary>
/// Defines the properties that specify the presentation of an element.
/// </summary>
public partial class Style
{
    /// <summary>
    /// Determines whether this node is visible.
    /// </summary>
    public bool? IsVisible { get; set; }

    /// <summary>
    /// <para>
    /// The anchor defines the point of origin of the element.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Modifying this property will trigger a reflow of the layout. This is a
    /// computationally expensive operation and should be done sparingly.
    /// </remarks>
    public Anchor? Anchor { get; set; }

    /// <summary>
    /// Defines the flow direction of the node.
    /// </summary>
    public Flow? Flow { get; set; }

    /// <summary>
    /// Defines the gap between nodes.
    /// </summary>
    public int? Gap { get; set; }
}
