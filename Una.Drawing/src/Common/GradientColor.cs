/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public class GradientColor(
    Color?       color1 = null,
    Color?       color2 = null,
    GradientType type   = GradientType.Vertical
)
{
    /// <summary>
    /// The top-left color of the gradient.
    /// </summary>
    public Color? Color1 { get; set; } = color1;

    /// <summary>
    /// The top-right color of the gradient.
    /// </summary>
    public Color? Color2 { get; set; } = color2;

    /// <summary>
    /// Defines the type of gradient to render.
    /// </summary>
    public GradientType Type { get; set; } = type;

    public static GradientColor Horizontal(Color? start, Color? end) =>
        new(start, end, GradientType.Horizontal);

    public static GradientColor Vertical(Color? start, Color? end) =>
        new(start, end, GradientType.Vertical);

    public static GradientColor Radial(Color? start, Color? end) =>
        new(start, end, GradientType.Radial);

    /// <summary>
    /// Returns true if all color points are undefined or invisible.
    /// </summary>
    public bool IsEmpty => !((Color1?.IsVisible ?? false) || (Color2?.IsVisible ?? false));

    public override string ToString() => $"GradientColor({Enum.GetName(Type)}, {Color1}, {Color2})";
}

public enum GradientType
{
    Horizontal,
    Vertical,
    Radial,
}
