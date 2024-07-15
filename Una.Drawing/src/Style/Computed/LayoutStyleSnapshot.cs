/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

[StructLayout(LayoutKind.Sequential)]
internal struct LayoutStyleSnapshot
{
    internal Anchor.AnchorPoint Anchor;
    internal bool               IsVisible;
    internal int                Width;
    internal int                Height;
    internal Flow               Flow;
    internal int                Gap;
    internal bool               Stretch;
    internal int                PaddingTop;
    internal int                PaddingRight;
    internal int                PaddingBottom;
    internal int                PaddingLeft;
    internal int                MarginTop;
    internal int                MarginRight;
    internal int                MarginBottom;
    internal int                MarginLeft;
    internal bool               WordWrap;
    internal uint               Font;
    internal int                FontSize;
    internal float              LineHeight;
    internal bool               TextOverflow;

    internal static LayoutStyleSnapshot Create(ref ComputedStyle style)
    {
        return new() {
            Anchor        = style.Anchor.Point,
            IsVisible     = style.IsVisible,
            Width         = style.Size.Width,
            Height        = style.Size.Height,
            Flow          = style.Flow,
            Gap           = style.Gap,
            Stretch       = style.Stretch,
            PaddingTop    = style.Padding.Top,
            PaddingRight  = style.Padding.Right,
            PaddingBottom = style.Padding.Bottom,
            PaddingLeft   = style.Padding.Left,
            MarginTop     = style.Margin.Top,
            MarginRight   = style.Margin.Right,
            MarginBottom  = style.Margin.Bottom,
            MarginLeft    = style.Margin.Left,
            WordWrap      = style.WordWrap,
            Font          = style.Font,
            FontSize      = style.FontSize,
            LineHeight    = style.LineHeight,
            TextOverflow  = style.TextOverflow
        };
    }
}
