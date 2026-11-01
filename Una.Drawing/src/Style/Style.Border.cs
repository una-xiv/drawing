namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// <para>
    /// Defines the color of the border around the node.
    /// </para>
    /// This property has no effect if <see cref="BorderWidth"/> is left at 0.
    /// </summary>
    public BorderColor? BorderColor { get => _borderColor; set => SetBorderColor(ref _borderColor, value); }
    private BorderColor? _borderColor;

    /// <summary>
    /// Defines the thickness of the border around the node.
    /// </summary>
    /// <remarks>
    /// This property has no effect if <see cref="BorderColor"/> is left
    /// undefined.
    /// </remarks>
    public EdgeSize? BorderWidth { get => _borderWidth; set => Set(ref _borderWidth, value); }
    private EdgeSize? _borderWidth;

    /// <summary>
    /// Defines the roundness of the corners of the node.
    /// </summary>
    public float? BorderRadius { get => _borderRadius; set => Set(ref _borderRadius, value); }
    private float? _borderRadius;

    /// <summary>
    /// Defines the inset of the border around the node, allowing the border to
    /// be drawn inside the node's bounds.
    /// </summary>
    public EdgeSize? BorderInset { get => _borderInset; set => Set(ref _borderInset, value); }
    private EdgeSize? _borderInset;

    /// <summary>
    /// <para>
    /// Defines the stroke color of the node. This property has no effect
    /// if <see cref="StrokeWidth"/> is left at 0.
    /// </para>
    /// <para>
    /// Compared to <see cref="BorderColor"/>, this property is used to define
    /// a single color stroke around the background of the node, while a border
    /// allows for different colors on each edge, as well as a different inset
    /// value.
    /// </para>
    /// </summary>
    public Color? StrokeColor { get => _strokeColor; set => SetColor(ref _strokeColor, value); }
    private Color? _strokeColor;

    /// <summary>
    /// <para>
    /// Defines the thickness of the stroke around the node.
    /// </para>
    /// <para>
    /// Compared to <see cref="BorderWidth"/>, this property is used to define
    /// a fixed stroke width around the background of the node, while a border
    /// border allows for different sizes on each edge, as well as a different
    /// inset value.
    /// </para>
    /// </summary>
    public float? StrokeWidth { get => _strokeWidth; set => Set(ref _strokeWidth, value); }
    private float? _strokeWidth;

    /// <summary>
    /// Similar to <see cref="BorderInset"/>, this property defines the inset
    /// distance of the stroke around the node, allowing the stroke to be drawn
    /// inside the node's bounds.
    /// </summary>
    public float? StrokeInset { get => _strokeInset; set => Set(ref _strokeInset, value); }
    private float? _strokeInset;

    /// <summary>
    /// Similar to <see cref="BorderRadius"/>, this property defines an override
    /// of the stroke the corner radius if it needs to be different from the
    /// border radius.
    /// </summary>
    public float? StrokeRadius { get => _strokeRadius; set => Set(ref _strokeRadius, value); }
    private float? _strokeRadius;

    /// <summary>
    /// Specifies which corners of the node should be rounded.
    /// </summary>
    public RoundedCorners? RoundedCorners { get => _roundedCorners; set => Set(ref _roundedCorners, value); }
    private RoundedCorners? _roundedCorners;
}
