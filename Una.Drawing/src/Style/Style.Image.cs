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
    /// Draws a game icon in the node.
    /// </summary>
    public uint? IconId { get; set; }

    /// <summary>
    /// A byte array of an image file to be displayed in the node.
    /// This property takes precedence over <see cref="IconId"/>.
    /// </summary>
    public byte[]? ImageBytes { get; set; }

    /// <summary>
    /// Defines the space between the icon and the border edges of the node.
    /// </summary>
    public EdgeSize? ImageInset { get; set; }

    /// <summary>
    /// A translation offset for the icon.
    /// </summary>
    public Vector2? ImageOffset { get; set; }

    /// <summary>
    /// Specifies the rounding of the icon.
    /// </summary>
    public float? ImageRounding { get; set; }

    /// <summary>
    /// Specifies the corners of the icon that should be rounded.
    /// </summary>
    public RoundedCorners? ImageRoundedCorners { get; set; }

    /// <summary>
    /// Whether the icon should be displayed in black and white.
    /// </summary>
    public bool? ImageGrayscale { get; set; }

    /// <summary>
    /// A contrast value for the icon. Must range between 0 and 1.
    /// </summary>
    public float? ImageContrast { get; set; }

    /// <summary>
    /// Defines the rotation of the image in degrees.
    /// </summary>
    public short? ImageRotation { get; set; }

    /// <summary>
    /// Defines the color to apply to the image.
    /// </summary>
    public Color? ImageColor { get; set; }

    /// <summary>
    /// Defines the blend mode to apply to the image. Use this in conjunction
    /// with <see cref="ImageColor"/>.
    /// </summary>
    public BlendMode? ImageBlendMode { get; set; }

    /// <summary>
    /// Defines the resource path of the ULD file to be displayed in the node.
    /// </summary>
    public string? UldResource { get; set; }

    /// <summary>
    /// Defines the ULD parts id to be displayed in the node.
    /// </summary>
    public int? UldPartsId { get; set; }

    /// <summary>
    /// Defines the ULD part id to be displayed in the node.
    /// </summary>
    public int? UldPartId { get; set; }
}
