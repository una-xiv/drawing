/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Buffers;
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using Una.Drawing.Generator;

namespace Una.Drawing;

internal static class Renderer
{
    private static List<IGenerator> _generators = [];

    private static SKSurface _skSurface = null!;
    private static SKCanvas  _skCanvas  = null!;

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

        // Determine maximum texture size.
        int maxDisplaySize = Math.Max((int)ImGui.GetIO().DisplaySize.X, (int)ImGui.GetIO().DisplaySize.Y);

        // Adhere to a minimum of 4k texture to accommodate overflowing container background textures.
        // This only increases in case of 4K displays that have a horizontal resolution of 5120 pixels.
        int maxTextureSize = Math.Max(8192, 8192);

        // Create the SKSurface and SKCanvas.
        SKImageInfo         info  = new(maxTextureSize, maxTextureSize);
        SKSurfaceProperties props = new(SKSurfacePropsFlags.None, SKPixelGeometry.Unknown);

        _skSurface  = SKSurface.Create(info, props);
        _skCanvas   = _skSurface.Canvas;
    }

    internal static void Dispose()
    {
        _skCanvas.Dispose();
        _skSurface.Dispose();
    }

    /// <summary>
    /// Creates a texture for the given node.
    /// </summary>
    internal static unsafe IDalamudTextureWrap? CreateTexture(Node node)
    {
        if (node.Width == 0 || node.Height == 0) return null;

        using SKPaint paint = new();
        paint.Color     = SKColor.Empty;
        paint.Style     = SKPaintStyle.Fill;
        paint.BlendMode = SKBlendMode.Clear;
        _skCanvas.DrawRect(0, 0, node.Width, node.Height, paint);

        foreach (IGenerator generator in _generators) {
            generator.Generate(_skCanvas, node);
        }

        byte[] targetData = ArrayPool<byte>.Shared.Rent(node.Width * node.Height * 4);

        fixed (void* ptr = targetData) {
            _skSurface.ReadPixels(
                new() {
                    Width      = node.Width,
                    Height     = node.Height,
                    AlphaType  = SKAlphaType.Premul,
                    ColorType  = SKColorType.Bgra8888,
                    ColorSpace = SKColorSpace.CreateSrgb()
                },
                (nint)ptr,
                node.Width * 4,
                0,
                0
            );
        }

        IDalamudTextureWrap texture = DalamudServices.TextureProvider.CreateFromRaw(
            RawImageSpecification.Rgba32(node.Width, node.Height),
            targetData
        );

        // IDalamudTextureWrap texture = DalamudServices.UiBuilder.LoadImageRaw(targetData, node.Width, node.Height, 4);

        ArrayPool<byte>.Shared.Return(targetData);

        return texture;
    }
}
