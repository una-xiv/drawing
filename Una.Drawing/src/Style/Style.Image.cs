using Dalamud.Game.Text.SeStringHandling;

namespace Una.Drawing;

public partial class Style
{
    /// <summary>
    /// Draws a game icon in the node.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="BitmapFontIcon"/>
    /// and <see cref="UldResource"/> and its associated properties.
    /// </remarks>
    public uint? IconId { get => _iconId; set => Set(ref _iconId, value); }
    private uint? _iconId;

    /// <summary>
    /// A byte array of an image file to be displayed in the node.
    /// This property takes precedence over <see cref="IconId"/>.
    /// </summary>
    public byte[]? ImageBytes { get => _imageBytes; set => Set(ref _imageBytes, value); }
    private byte[]? _imageBytes;

    /// <summary>
    /// Defines the space between the icon and the border edges of the node.
    /// </summary>
    public EdgeSize? ImageInset { get => _imageInset; set => Set(ref _imageInset, value); }
    private EdgeSize? _imageInset;

    /// <summary>
    /// A translation offset for the icon.
    /// </summary>
    public Vector2? ImageOffset { get => _imageOffset; set => Set(ref _imageOffset, value); }
    private Vector2? _imageOffset;

    /// <summary>
    /// Specifies the rounding of the icon.
    /// </summary>
    public float? ImageRounding { get => _imageRounding; set => Set(ref _imageRounding, value); }
    private float? _imageRounding;

    /// <summary>
    /// Specifies the corners of the icon that should be rounded.
    /// </summary>
    public RoundedCorners? ImageRoundedCorners { get => _imageRoundedCorners; set => Set(ref _imageRoundedCorners, value); }
    private RoundedCorners? _imageRoundedCorners;

    /// <summary>
    /// Whether the icon should be displayed in black and white.
    /// </summary>
    public bool? ImageGrayscale { get => _imageGrayscale; set => Set(ref _imageGrayscale, value); }
    private bool? _imageGrayscale;

    /// <summary>
    /// A contrast value for the icon. Must range between 0 and 1.
    /// </summary>
    public float? ImageContrast { get => _imageContrast; set => Set(ref _imageContrast, value); }
    private float? _imageContrast;

    /// <summary>
    /// Defines the rotation of the image in degrees.
    /// </summary>
    public short? ImageRotation { get => _imageRotation; set => Set(ref _imageRotation, value); }
    private short? _imageRotation;

    /// <summary>
    /// Defines the color to apply to the image.
    /// </summary>
    public Color? ImageColor { get => _imageColor; set => SetColor(ref _imageColor, value); }
    private Color? _imageColor;

    /// <summary>
    /// Defines the blend mode to apply to the image. Use this in conjunction
    /// with <see cref="ImageColor"/>.
    /// </summary>
    public BlendMode? ImageBlendMode { get => _imageBlendMode; set => Set(ref _imageBlendMode, value); }
    private BlendMode? _imageBlendMode;

    /// <summary>
    /// The scale mode to apply to the image. Defaults to
    /// <see cref="ImageScaleMode.Adapt"/>.
    /// </summary>
    public ImageScaleMode? ImageScaleMode { get => _imageScaleMode; set => Set(ref _imageScaleMode, value); }
    private ImageScaleMode? _imageScaleMode;
    
    /// <summary>
    /// Specifies if the image should be clamped or repeated.
    /// </summary>
    public ImageTileMode? ImageTileMode { get => _imageTileMode; set => Set(ref _imageTileMode, value); }
    private ImageTileMode? _imageTileMode;

    /// <summary>
    /// Defines the scale of the image. Defaults to 1.0f.
    /// </summary>
    public float? ImageScale { get => _imageScale; set => Set(ref _imageScale, value); }
    private float? _imageScale;
    
    /// <summary>
    /// Applies a gaussian blur to the image in the X and Y axis.
    /// </summary>
    public Vector2? ImageBlur { get => _imageBlur; set => Set(ref _imageBlur, value); }
    private Vector2? _imageBlur;
    
    /// <summary>
    /// Defines the bitmap font icon to render.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="IconId"/> and
    /// <see cref="UldResource"/> and its associated properties.
    /// </remarks>
    public BitmapFontIcon? BitmapFontIcon { get => _bitmapFontIcon; set => Set(ref _bitmapFontIcon, value); }
    private BitmapFontIcon? _bitmapFontIcon;
    
    /// <summary>
    /// Defines the resource path of the ULD file to be displayed in the node.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="IconId"/>
    /// and <see cref="BitmapFontIcon"/>.
    /// </remarks>
    public string? UldResource { get => _uldResource; set => Set(ref _uldResource, value); }
    private string? _uldResource;

    /// <summary>
    /// The ULD style to be displayed in the node.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="IconId"/>
    /// and <see cref="BitmapFontIcon"/>.
    /// </remarks>
    public UldStyle? UldStyle { get => _uldStyle; set => Set(ref _uldStyle, value); }
    private UldStyle? _uldStyle;

    /// <summary>
    /// Defines the ULD parts id to be displayed in the node.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="IconId"/>
    /// and <see cref="BitmapFontIcon"/>.
    /// </remarks>
    public int? UldPartsId { get => _uldPartsId; set => Set(ref _uldPartsId, value); }
    private int? _uldPartsId;

    /// <summary>
    /// Defines the ULD part id to be displayed in the node.
    /// </summary>
    /// <remarks>
    /// This value is mutually exclusive with <see cref="IconId"/>
    /// and <see cref="BitmapFontIcon"/>.
    /// </remarks>
    public int? UldPartId { get => _uldPartId; set => Set(ref _uldPartId, value); }
    private int? _uldPartId;
    
    /// <summary>
    /// Adds a drop shadow to an image.
    /// The elements of the vector are defined as follows:
    /// <list type="bullet">
    ///   <item>X: The X offset of the shadow.</item>
    ///   <item>Y: The Y offset of the shadow.</item>
    ///   <item>Z: The horizontal sigma of the shadow.</item>
    ///   <item>W: The vertical sigma of the shadow.</item>
    /// </list>
    /// </summary>
    public Vector4? DropShadow { get => _dropShadow; set => Set(ref _dropShadow, value); }
    private Vector4? _dropShadow;
}