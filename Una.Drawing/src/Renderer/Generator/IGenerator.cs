/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing.Generator;

internal interface IGenerator
{
    /// <summary>
    /// Defines the order in which the generator should be rendered.
    /// </summary>
    public int RenderOrder { get; }

    /// <summary>
    /// Generates a part of the texture for the given node.
    /// </summary>
    /// <param name="canvas">The drawing canvas.</param>
    /// <param name="node">The source node.</param>
    public void Generate(SKCanvas canvas, Node node);
}
