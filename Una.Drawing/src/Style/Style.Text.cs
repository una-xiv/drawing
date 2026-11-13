namespace Una.Drawing;

/// <summary>
/// Defines the properties that specify the presentation of a node.
/// </summary>
public partial class Style
{
    /// <summary>
    /// Defines the color representation of text contents.
    /// </summary>
    public Color? Color { get => _color; set => SetColor(ref _color, value); }
    private Color? _color;

    /// <summary>
    /// Defines the outline color of text contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property does not have an effect if <see cref="OutlineSize"/> is
    /// left at 0.
    /// </para>
    /// </remarks>
    public Color? OutlineColor { get => _outlineColor; set => SetColor(ref _outlineColor, value); }
    private Color? _outlineColor;

    /// <summary>
    /// Defines the outline thickness of text contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property has no effect if <see cref="OutlineColor"/> is left
    /// undefined.
    /// </para>
    /// </remarks>
    public int? OutlineSize { get => _outlineSize; set => Set(ref _outlineSize, value); }
    private int? _outlineSize;

    /// <summary>
    /// Defines which font to use when rendering text content. This property
    /// references a font by its ID which has been registered using the
    /// <see cref="FontRegistry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modifying this property will trigger a reflow of the layout. This is a
    /// computationally expensive operation and should be done sparingly.
    /// </para>
    /// </remarks>
    public uint? Font { get => _font; set => Set(ref _font, value); }
    private uint? _font;

    /// <summary>
    /// Specifies the size of the font used to render text contents.
    /// </summary>
    public int? FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    private int? _fontSize;

    /// <summary>
    /// Defines the line height of multi-line text. This value is a multiplier
    /// of the height of the font.
    /// </summary>
    public float? LineHeight { get => _lineHeight; set => Set(ref _lineHeight, value); }
    private float? _lineHeight;

    /// <summary>
    /// Specifies the alignment of text contents within the bounds of the node.
    /// Defaults to <see cref="Anchor.TopLeft"/>.
    /// </summary>
    public Anchor? TextAlign { get => _textAlign; set => Set(ref _textAlign, value); }
    private Anchor? _textAlign;

    /// <summary>
    /// Defines the offset of the text content from the top-left corner of the
    /// inner bounds of the node.
    /// </summary>
    /// <remarks>
    /// This property does not affect the automatically calculated size of the
    /// node, but is purely a visual offset that can be used if some fonts do
    /// not align correctly.
    /// </remarks>
    public Vector2? TextOffset { get => _textOffset; set => Set(ref _textOffset, value); }
    private Vector2? _textOffset;

    /// <summary>
    /// Whether text is allowed to overflow the bounds of the node. When set to
    /// <c>false</c>, text will be truncated and an ellipsis will be appended to
    /// the end of the text. If <see cref="WordWrap"/> is enabled, text will be
    /// split into multiple lines instead. Defaults to <c>true</c> for best
    /// performance.
    /// </summary>
    public bool? TextOverflow { get => _textOverflow; set => Set(ref _textOverflow, value); }
    private bool? _textOverflow;

    /// <summary>
    /// Defines a maximum width for nodes containing text. If the text exceeds
    /// the given width, it will be truncated and an ellipsis will be appended
    /// to the end of the text. Giving this property a value that is not NULL
    /// and higher than 0 will implicitly disable <see cref="TextOverflow"/>
    /// and <see cref="WordWrap"/>.
    /// </summary>
    public float? MaxWidth { get => _maxWidth; set => Set(ref _maxWidth, value); }
    private float? _maxWidth;

    /// <summary>
    /// <para>
    /// Whether to wrap text contents to the next line when they exceed the
    /// defined width of the node.
    /// </para>
    /// <para>
    /// If WordWrap is set to false and the text would exceed the width of the
    /// element, it will be truncated and an ellipsis will be appended to the
    /// end of the text.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Modifying this property will trigger a reflow of the layout. This is a
    /// computationally expensive operation and should be done sparingly.
    /// </remarks>
    public bool? WordWrap { get => _wordWrap; set => Set(ref _wordWrap, value); }
    private bool? _wordWrap;

    /// <summary>
    /// Specifies the blur radius of the shadow cast by the text content.
    /// </summary>
    public float? TextShadowSize { get => _textShadowSize; set => Set(ref _textShadowSize, value); }
    private float? _textShadowSize;

    /// <summary>
    /// Defines the color of the shadow cast by the text content.
    /// </summary>
    public Color? TextShadowColor { get => _textShadowColor; set => SetColor(ref _textShadowColor, value); }
    private Color? _textShadowColor;
}