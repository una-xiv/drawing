namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// <para>
    /// Defines a single background color for the element.
    /// </para>
    /// <para>
    /// If both <see cref="BackgroundColor"/> and <see cref="BackgroundGradient"/>
    /// are defined, the gradient will be rendered on top of the background color.
    /// </para>
    /// </summary>
    public Color? BackgroundColor { get => _backgroundColor; set => SetColor(ref _backgroundColor, value); }
    private Color? _backgroundColor;

    /// <summary>
    /// <para>
    /// Defines a 4-point gradient background for the element.
    /// </para>
    /// <para>
    /// If both <see cref="BackgroundColor"/> and <see cref="BackgroundGradient"/>
    /// are defined, the gradient will be rendered on top of the background color.
    /// </para>
    /// </summary>
    public GradientColor? BackgroundGradient { get => _backgroundGradient; set => SetGradientColor(ref _backgroundGradient, value); }
    private GradientColor? _backgroundGradient;

    /// <summary>
    /// Defines the inset of the gradient background which determines the space
    /// between the gradient background and the element's border.
    /// </summary>
    public EdgeSize? BackgroundGradientInset { get => _backgroundGradientInset; set => Set(ref _backgroundGradientInset, value); }
    private EdgeSize? _backgroundGradientInset;

    /// <summary>
    /// An image to be displayed in the background of the element. Can be either
    /// a byte array of an image file or an icon ID.
    /// </summary>
    public object? BackgroundImage { get => _backgroundImage; set => Set(ref _backgroundImage, value); }
    private object? _backgroundImage;

    /// <summary>
    /// Defines the inset of the background image which determines the space
    /// between the background image and the element's border.
    /// </summary>
    public EdgeSize? BackgroundImageInset { get => _backgroundImageInset; set => Set(ref _backgroundImageInset, value); }
    private EdgeSize? _backgroundImageInset;

    /// <summary>
    /// Specifies the scale of the background image. Higher values will make the
    /// image appear more often in the background.
    /// </summary>
    public Vector2? BackgroundImageScale { get => _backgroundImageScale; set => Set(ref _backgroundImageScale, value); }
    private Vector2? _backgroundImageScale;

    /// <summary>
    /// Specifies the color to apply to the background image. Defaults to white.
    /// </summary>
    public Color? BackgroundImageColor { get => _backgroundImageColor; set => SetColor(ref _backgroundImageColor, value); }
    private Color? _backgroundImageColor;

    /// <summary>
    /// Specifies the rotation of the background image in degrees.
    /// </summary>
    public short? BackgroundImageRotation { get => _backgroundImageRotation; set => Set(ref _backgroundImageRotation, value); }
    private short? _backgroundImageRotation;

    /// <summary>
    /// Defines the color blend mode to apply to the background image.
    /// This works in conjunction with <see cref="BackgroundImageColor"/>.
    /// </summary>
    public BlendMode? BackgroundImageBlendMode { get => _backgroundImageBlendMode; set => Set(ref _backgroundImageBlendMode, value); }
    private BlendMode? _backgroundImageBlendMode;
}
