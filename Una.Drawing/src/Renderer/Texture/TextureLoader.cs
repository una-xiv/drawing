using System.Reflection;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Data.Files;

namespace Una.Drawing.Texture;

internal struct UldIcon
{
    public SKImage Texture { get; set; }
    public Rect Rect { get; set; }
    public Vector2 Size { get; set; }
}

internal static class TextureLoader
{
    private static readonly Dictionary<uint, TexFile>               IconToTexFileCache   = [];
    private static readonly Dictionary<uint, SKImage>               IconToImageCache     = [];
    private static readonly Dictionary<string, TexFile>             PathToTexFileCache   = [];
    private static readonly Dictionary<string, UldFile>             PathToUldFileCache   = [];

    /// <summary>
    /// Loads an embedded texture from one of the plugin assemblies.
    /// </summary>
    /// <param name="name">The logical name of the resource.</param>
    /// <returns>An instance of <see cref="IDalamudTextureWrap"/> that wraps the resource.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IDalamudTextureWrap GetEmbeddedTexture(string name)
    {
        return DalamudServices.TextureProvider.GetFromManifestResource(Assembly.GetExecutingAssembly(), name).GetWrapOrEmpty();
    }

    /// <summary>
    /// Gets the uld icon for the specified part ID.
    /// </summary>
    /// <param name="uldPath">The uld path to get for specifically the .uld file in question</param>
    /// <param name="partsId">What parts group to look for</param>
    /// <param name="partId">What part to use of parts group</param>
    /// <returns><see cref="UldIcon"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static UldIcon? LoadUld(string uldPath, int partsId, int partId)
    {
        if (uldPath.EndsWith(".uld") || uldPath.EndsWith(".tex"))
            uldPath = uldPath[..^4];

        var uldFile = LoadUldFile(uldPath + ".uld");
        var texFile = TextureLoader.LoadTexture(uldPath + "_hr1.tex");

        if (uldFile == null || texFile == null)
            return null;

        var part = uldFile.Parts[partsId];
        var subPart = part.Parts[partId];

        var uv = new Vector2(subPart.U, subPart.V) * 2;
        var size = new Vector2(subPart.W, subPart.H) * 2;

        return new UldIcon { Size = size, Texture = texFile, Rect = new Rect(uv, uv + size) };
    }

    internal static UldFile? LoadUldFile(string path)
    {
        if (DalamudServices.DataManager == null || DalamudServices.TextureProvider == null)
            throw new InvalidOperationException("Una.Drawing.DrawingLib has not been set-up.");

        if (!PathToUldFileCache.TryGetValue(path, out var uldFile))
        {
            uldFile = DalamudServices.DataManager.GetFile<UldFile>(path);
            if (uldFile == null) return null;
            PathToUldFileCache[path] = uldFile;
        }

        return uldFile;
    }

    internal static SKImage? LoadFromBytes(byte[] bytes)
    {
        using MemoryStream stream = new(bytes);
        using SKImage      image  = SKImage.FromEncodedData(stream);
        using SKBitmap     bitmap = SKBitmap.FromImage(image);

        // We need to do some fuckery to swap from BGRA to RGBA...
        SKImageInfo info = new(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

        IntPtr pixelPtr = Marshal.AllocHGlobal(bitmap.ByteCount);

        Marshal.Copy(bitmap.Bytes, 0, pixelPtr, bitmap.ByteCount);
        SKImage? output = SKImage.FromPixels(info, pixelPtr);

        return output;
    }

    internal static SKImage? LoadIcon(uint iconId)
    {
        if (IconToImageCache.TryGetValue(iconId, out SKImage? cachedImage)) return cachedImage;

        TexFile iconFile = GetIconFile(iconId);

        SKImageInfo info = new(iconFile.Header.Width, iconFile.Header.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

        IntPtr pixelPtr = Marshal.AllocHGlobal(iconFile.ImageData.Length);
        Marshal.Copy(iconFile.ImageData, 0, pixelPtr, iconFile.ImageData.Length);

        using SKPixmap pixmap = new(info, pixelPtr);
        SKImage?       image  = SKImage.FromPixels(pixmap);

        IconToImageCache[iconId] = image;

        return image;
    }

    internal static SKImage? LoadTexture(string path)
    {
        if (DalamudServices.DataManager == null || DalamudServices.TextureProvider == null)
            throw new InvalidOperationException("Una.Drawing.DrawingLib has not been set-up.");

        if (!PathToTexFileCache.TryGetValue(path, out var texFile))
        {
            path = DalamudServices.TextureSubstitutionProvider.GetSubstitutedPath(path);

            texFile = Path.IsPathRooted(path)
                ? DalamudServices.DataManager.GameData.GetFileFromDisk<TexFile>(path)
                : DalamudServices.DataManager.GetFile<TexFile>(path);

            if (null == texFile) return null;

            PathToTexFileCache[path] = texFile;
        }

        SKImageInfo info = new(texFile.Header.Width, texFile.Header.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        IntPtr pixelPtr = Marshal.AllocHGlobal(texFile.ImageData.Length);
        Marshal.Copy(texFile.ImageData, 0, pixelPtr, texFile.ImageData.Length);

        using SKPixmap pixmap = new(info, pixelPtr);
        SKImage?       image  = SKImage.FromPixels(pixmap);

        return image;
    }

    /// <summary>
    /// Returns a <see cref="TexFile"/> for the given icon ID.
    /// </summary>
    private static TexFile GetIconFile(uint iconId)
    {
        if (IconToTexFileCache.TryGetValue(iconId, out var cachedIconFile)) return cachedIconFile;

        if (DalamudServices.DataManager == null || DalamudServices.TextureProvider == null)
            throw new InvalidOperationException("Una.Drawing.DrawingLib has not been set-up.");

        string originalIconPath = DalamudServices.TextureProvider.GetIconPath(iconId)
            ?? throw new InvalidOperationException($"Failed to get icon path for #{iconId}.");

        string iconPath = DalamudServices.TextureSubstitutionProvider.GetSubstitutedPath(originalIconPath);

        TexFile? iconFile = Path.IsPathRooted(iconPath)
            ? DalamudServices.DataManager.GameData.GetFileFromDisk<TexFile>(iconPath)
            : DalamudServices.DataManager.GetFile<TexFile>(iconPath);

        IconToTexFileCache[iconId] = iconFile
            ?? throw new InvalidOperationException($"Failed to load icon file for #{iconId}.");

        return iconFile;
    }
}
