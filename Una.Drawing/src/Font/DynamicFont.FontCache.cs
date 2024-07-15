namespace Una.Drawing.Font;

internal partial class DynamicFont
{
    private Dictionary<int, SKFont> TextFontCache  { get; } = [];
    private Dictionary<int, SKFont> GlyphFontCache { get; } = [];

    private SKFont GetTextFont(int fontSize)
    {
        if (TextFontCache.TryGetValue(fontSize, out SKFont? cachedFont)) return cachedFont;

        var font = new SKFont(TextTypeface, fontSize + SizeOffset);
        font.Hinting  = SKFontHinting.Full;
        font.Edging   = SKFontEdging.SubpixelAntialias;
        font.Subpixel = false;
        font.Embolden = false;

        TextFontCache[fontSize] = font;

        return font;
    }

    private SKFont GetGlyphFont(int fontSize)
    {
        if (GlyphFontCache.TryGetValue(fontSize, out SKFont? cachedFont)) return cachedFont;

        var font = new SKFont(GlyphTypeface, fontSize + SizeOffset);
        font.Hinting  = SKFontHinting.Full;
        font.Edging   = SKFontEdging.SubpixelAntialias;
        font.Subpixel = false;
        font.Embolden = false;

        GlyphFontCache[fontSize] = font;

        return font;
    }


}
