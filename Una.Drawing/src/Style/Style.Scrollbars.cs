namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// Specifies the color of the scrollbar tracks.
    /// </summary>
    public Color? ScrollbarTrackColor { get => _scrollbarTrackColor; set => SetColor(ref _scrollbarTrackColor, value); }
    private Color? _scrollbarTrackColor;

    /// <summary>
    /// Specifies the color of the scrollbar thumbs.
    /// </summary>
    public Color? ScrollbarThumbColor { get => _scrollbarThumbColor; set => SetColor(ref _scrollbarThumbColor, value); }
    private Color? _scrollbarThumbColor;

    /// <summary>
    /// Specifies the color of the scrollbar thumbs when hovered.
    /// </summary>
    public Color? ScrollbarThumbHoverColor { get => _scrollbarThumbHoverColor; set => SetColor(ref _scrollbarThumbHoverColor, value); }
    private Color? _scrollbarThumbHoverColor;

    /// <summary>
    /// Specifies the color of the scrollbar thumbs when active.
    /// </summary>
    public Color? ScrollbarThumbActiveColor { get => _scrollbarThumbActiveColor; set => SetColor(ref _scrollbarThumbActiveColor, value); }
    private Color? _scrollbarThumbActiveColor;
}
