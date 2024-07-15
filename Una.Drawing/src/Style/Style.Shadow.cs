/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// Defines the shadow size on the node for each side.
    /// </summary>
    public EdgeSize? ShadowSize { get; set; }

    /// <summary>
    /// Defines the inset of the shadow on the node in pixels.
    /// </summary>
    public int? ShadowInset { get; set; }

    /// <summary>
    /// Defines the offset of the shadow on the node in pixels.
    /// </summary>
    public Vector2? ShadowOffset { get; set; }
}
