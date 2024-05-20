﻿/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Una.Drawing.Font;

internal sealed class FontNativeImpl : IFont
{
    private readonly SKTypeface              _typeface;
    private readonly Dictionary<int, SKFont> _fontCache = [];
    private readonly float                   _sizeOffset;

    internal FontNativeImpl(SKTypeface typeface, float sizeOffset)
    {
        _typeface   = typeface;
        _sizeOffset = sizeOffset;
    }

    /// <inheritdoc/>
    public float GetLineHeight(int fontSize) => (96 / 72) * GetFont(fontSize).Size;

    /// <inheritdoc/>
    public MeasuredText MeasureText(
        string text,
        int    fontSize         = 14,
        float? maxLineWidth     = null,
        bool   wordWrap         = false,
        bool   textOverflow     = true,
        float  lineHeightFactor = 1.2f
    )
    {
        var maxWidth   = 0;
        var maxHeight  = 0;
        var font       = GetFont(fontSize);
        var lineHeight = (int)Math.Ceiling(GetLineHeight(fontSize));

        if (textOverflow || maxLineWidth is null or 0) {
            maxWidth = (int)Math.Ceiling(font.MeasureText(text));

            return new() {
                Size      = new(maxWidth, lineHeight),
                Lines     = [text],
                LineCount = 1,
            };
        }

        if (wordWrap == false) {
            int charCount = font.BreakText(text, maxLineWidth.Value);

            if (charCount < text.Length) {
                text = text[..(Math.Max(0, charCount - 2))] + "…";
            }

            return new() {
                Size      = new((int)maxLineWidth.Value, lineHeight),
                Lines     = [text],
                LineCount = 1,
            };
        }

        List<string> lines = [];

        int totalChars = text.Length;
        var usedChars  = 0;

        for (var i = 0; i < totalChars; i++) {
            int    chunkSize = font.BreakText(text, maxLineWidth.Value);
            string chunk     = text[..chunkSize];
            int    lastSpace = chunk.LastIndexOf(' ');

            chunk     = chunk[..(lastSpace == -1 ? chunkSize : lastSpace)];
            chunkSize = chunk.Length;

            i         += chunkSize;
            usedChars += chunkSize;
            text      =  text[chunkSize..];

            if (chunk.Length > 0) {
                lines.Add(chunk.Trim());
                maxWidth  =  (int)Math.Ceiling(Math.Max(maxWidth, font.MeasureText(chunk)));
                maxHeight += lines.Count < 2 ? lineHeight : (int)(lineHeight * lineHeightFactor);
            }
        }

        if (usedChars < totalChars) {
            string? lastLine  = lines.LastOrDefault();
            float   lastWidth = lastLine is not null ? font.MeasureText(lastLine) : 0;

            if (lastWidth == 0 || lastWidth + font.MeasureText(text) > maxLineWidth) {
                lines.Add(text.Trim());
                maxWidth  =  (int)Math.Ceiling(Math.Max(maxWidth, font.MeasureText(text)));
                maxHeight += lines.Count < 2 ? lineHeight : (int)(lineHeight * lineHeightFactor);
            } else {
                lines[^1] += text;
                maxWidth  =  (int)Math.Ceiling(Math.Max(maxWidth, font.MeasureText(lines[^1])));
            }
        }

        return new() {
            Size      = new(maxWidth, maxHeight),
            Lines     = lines.ToArray(),
            LineCount = (uint)lines.Count,
        };
    }

    /// <inheritdoc/>
    public void DrawText(SKCanvas canvas, SKPaint paint, SKPoint pos, int fontSize, string text)
    {
        canvas.DrawText(text, pos.X, pos.Y, GetFont(fontSize), paint);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (SKFont font in _fontCache.Values) {
            font.Dispose();
        }

        _fontCache.Clear();
    }

    public SKFontMetrics GetMetrics(int fontSize) => GetFont(fontSize).Metrics;

    private SKFont GetFont(int fontSize)
    {
        if (_fontCache.TryGetValue(fontSize, out SKFont? cachedFont)) return cachedFont;

        var font = new SKFont(_typeface, fontSize + _sizeOffset);
        font.Hinting  = SKFontHinting.Full;
        font.Edging   = SKFontEdging.SubpixelAntialias;
        font.Subpixel = false;
        font.Embolden = false;

        _fontCache[fontSize] = font;

        return font;
    }
}
