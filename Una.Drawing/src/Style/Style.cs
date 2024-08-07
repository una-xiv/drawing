﻿/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

/// <summary>
/// Defines the properties that specify the presentation of an element.
/// </summary>
public sealed partial class Style
{
    /// <summary>
    /// Specifies the opacity of the node. Must be a value between 0 and 1.
    /// </summary>
    public float? Opacity { get; set; }

    /// <summary>
    /// Whether the node's texture should be antialiased. This applies to
    /// background images, icons, and borders. Text is always antialiased,
    /// regardless of this setting.
    /// </summary>
    public bool? IsAntialiased { get; set; }
}
