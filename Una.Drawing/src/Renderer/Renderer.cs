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

    private static nint _pixelDataPtr;

    private static readonly SKColorSpace SkColorSpace = SKColorSpace.CreateSrgb();

    internal static unsafe void Setup()
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

        Tuple<nint> handle = DalamudServices.PluginInterface.GetOrCreateData(
            "Una.Drawing.Framebuffer",
            () =>
            {
                var data   = new byte[8192 * 8192 * 4];
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                return Tuple.Create(handle.AddrOfPinnedObject());
            }
        );

        _pixelDataPtr = handle.Item1;
    }

    internal static void Dispose()
    {
        SkColorSpace.Dispose();
    }

    /// <summary>
    /// Creates a texture for the given node.
    /// </summary>
    internal static unsafe IDalamudTextureWrap? CreateTexture(Node node)
    {
        if (node.OuterWidth == 0 || node.OuterHeight == 0) return null;
        if (node.OuterWidth > 8192 || node.OuterHeight > 8192) return null;

        SKImageInfo info    = new(node.OuterWidth, node.OuterHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SkColorSpace);
        using var   pixmap  = new SKPixmap(info, _pixelDataPtr);
        using var   surface = SKSurface.Create(pixmap);

        surface.Canvas.Clear();

        bool hasDrawn = false;

        foreach (IGenerator generator in _generators)
        {
            if (generator.Generate(surface.Canvas, node))
            {
                hasDrawn = true;
            }
        }

        if (!hasDrawn) return null;

        return DalamudServices.TextureProvider.CreateFromRaw(
            RawImageSpecification.Rgba32(node.OuterWidth, node.OuterHeight),
            pixmap.GetPixelSpan()
        );
    }
}