/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.IO;
using System.Linq;
using SkiaSharp;

namespace Una.Drawing.Font;

internal class FontFactory
{
    internal static IFont CreateFromFontFamily(
        string fontFamily,
        float sizeOffset = 0
    )
    {
        SKFontStyleSet styles    = SKFontManager.Default.GetFontStyles(fontFamily);
        SKFontStyle    fontStyle = styles.FirstOrDefault(
                style => style.Weight >= 400
                    && style.Slant == SKFontStyleSlant.Upright
                )
            ?? (styles.FirstOrDefault() ?? new());

        SKTypeface  srcTypeface = SKTypeface.FromFamilyName(fontFamily, fontStyle);

        return new DynamicFont(srcTypeface, FontRegistry.Glyphs, sizeOffset);
    }

    internal static IFont CreateFromFontFile(FileInfo file, float sizeOffset)
    {
        return new DynamicFont(SKTypeface.FromFile(file.FullName), FontRegistry.Glyphs, sizeOffset);
    }

    internal static IFont CreateFromFontStream(Stream stream, float sizeOffset)
    {
        return new DynamicFont(SKTypeface.FromStream(stream), FontRegistry.Glyphs, sizeOffset);
    }
}
