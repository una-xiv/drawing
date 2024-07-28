/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using System.Linq;
using System.Reflection;
using Una.Drawing.Generator;

namespace Una.Drawing;

internal static class Renderer
{
    private static List<IGenerator> _generators = [];

    private static byte[] _pixelData = null!;
    private static GCHandle _pixelHandle;
    private static nint _pixelDataPtr;
    private static SKColorSpace _skColorSpace = SKColorSpace.CreateSrgb();

    internal static void Setup()
    {
        // Collect generators.
        List<Type> generatorTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IGenerator)))
            .ToList();

        _generators = generatorTypes
            .Select(t => (IGenerator)Activator.CreateInstance(t)!)
            .OrderBy(g => g.RenderOrder)
        .ToList();

        _pixelData = new byte[8192 * 8192 * 4];
        _pixelHandle = GCHandle.Alloc(_pixelData, GCHandleType.Pinned);
        _pixelDataPtr = _pixelHandle.AddrOfPinnedObject();
    }

    internal static void Dispose()
    {
        _skColorSpace.Dispose();
        _pixelHandle.Free();
    }

    /// <summary>
    /// Creates a texture for the given node.
    /// </summary>
    internal static unsafe IDalamudTextureWrap? CreateTexture(Node node)
    {
        if (node.Width == 0 || node.Height == 0) return null;
        if (node.Width > 8192 || node.Height > 8192) return null;

        SKImageInfo info = new(node.Width, node.Height, SKColorType.Bgra8888, SKAlphaType.Premul, _skColorSpace);
        using var pixmap = new SKPixmap(info, _pixelDataPtr);
        using var surface = SKSurface.Create(pixmap);

        surface.Canvas.Clear();

        bool hasDrawn = false;
        foreach (IGenerator generator in _generators) {
            if (generator.Generate(surface.Canvas, node)) {
                hasDrawn = true;
            }
        }

        if (!hasDrawn) return null;

        return DalamudServices.TextureProvider.CreateFromRaw(
            RawImageSpecification.Rgba32(node.Width, node.Height),
            _pixelData
        );
    }
}