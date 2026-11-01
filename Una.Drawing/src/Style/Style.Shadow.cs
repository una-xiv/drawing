namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// Defines the shadow size on the node for each side.
    /// </summary>
    public EdgeSize? ShadowSize { get => _shadowSize; set => Set(ref _shadowSize, value); }
    private EdgeSize? _shadowSize;

    /// <summary>
    /// Defines the inset of the shadow on the node in pixels.
    /// </summary>
    public int? ShadowInset { get => _shadowInset; set => Set(ref _shadowInset, value); }
    private int? _shadowInset;

    /// <summary>
    /// Defines the offset of the shadow on the node in pixels.
    /// </summary>
    public Vector2? ShadowOffset { get => _shadowOffset; set => Set(ref _shadowOffset, value); }
    private Vector2? _shadowOffset;
}
