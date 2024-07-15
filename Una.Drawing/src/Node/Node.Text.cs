/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Una.Drawing.Font;

namespace Una.Drawing;

public partial class Node
{
    private EdgeSize _textCachedPadding = new();
    private object?  _textCachedNodeValue;
    private uint?    _textCachedFontId;
    private float?   _textCachedFontSize;
    private bool?    _textCachedWordWrap;
    private Size?    _textCachedNodeSize;

    internal MeasuredText? NodeValueMeasurement { get; private set; }

    /// <summary>
    /// <para>
    /// Computes the <see cref="NodeBounds.ContentSize"/> of this node based on
    /// the text content of the node and returns a <see cref="Size"/> object
    /// containing the width and height of the content.
    /// </para>
    /// <para>
    /// If the node value is empty, the returned size is zero.
    /// </para>
    /// <para>
    /// This function returns cached values if any style properties that affect
    /// the text content have not changed since the last computation.
    /// </para>
    /// </summary>
    private Size ComputeContentSizeFromText()
    {
        if (_nodeValue is SeString) {
            return ComputeContentSizeFromSeString();
        }

        if (_nodeValue is not string str || string.IsNullOrEmpty(str)) {
            return new(0, 0);
        }

        if (false == MustRecomputeNodeValue()) {
            return NodeValueMeasurement?.Size ?? new();
        }

        _texture = null;

        _textCachedNodeValue = _nodeValue;
        _textCachedWordWrap  = ComputedStyle.WordWrap;
        _textCachedPadding   = ComputedStyle.Padding.Copy();
        _textCachedFontId    = ComputedStyle.Font;
        _textCachedFontSize  = ComputedStyle.FontSize;
        _textCachedNodeSize  = ComputedStyle.Size.Copy();

        var font = FontRegistry.Fonts[ComputedStyle.Font];

        NodeValueMeasurement = font.MeasureText(
            str,
            ComputedStyle.FontSize,
            ComputedStyle.OutlineSize,
            ComputedStyle.Size.Width,
            ComputedStyle.WordWrap,
            ComputedStyle.TextOverflow,
            ComputedStyle.LineHeight
        );

        return NodeValueMeasurement.Value.Size;
    }

    private Size ComputeContentSizeFromSeString()
    {
        if (_nodeValue is not SeString str || str.Payloads.Count == 0) {
            return new(0, 0);
        }

        IFont font       = FontRegistry.Fonts[ComputedStyle.Font];
        var   maxWidth   = 0;
        var   maxHeight  = 0;
        Size  charSize   = font.MeasureText(" ", ComputedStyle.FontSize, ComputedStyle.OutlineSize).Size;
        int   spaceWidth = charSize.Width;

        foreach (var payload in str.Payloads) {
            switch (payload) {
                case TextPayload text:
                    if (string.IsNullOrEmpty(text.Text)) continue;

                    MeasuredText measurement = font.MeasureText(text.Text, ComputedStyle.FontSize, ComputedStyle.OutlineSize);
                    maxWidth  += measurement.Size.Width;
                    maxHeight =  Math.Max(maxHeight, measurement.Size.Height);
                    continue;
                case IconPayload:
                    maxWidth  += spaceWidth + 20 + spaceWidth;
                    continue;
            }
        }

        NodeValueMeasurement = new() {
            Lines     = [],
            LineCount = 1,
            Size      = new(maxWidth, maxHeight)
        };

        return new(maxWidth, maxHeight);
    }

    /// <summary>
    /// Returns true if the computed text content and bounding box based on the
    /// node value must be recomputed.
    /// </summary>
    private bool MustRecomputeNodeValue()
    {
        if (_nodeValue is null && !(NodeValueMeasurement?.Size.IsZero ?? false)) {
            NodeValueMeasurement = new();
            return false;
        }

        return _nodeValue is not null
            && (
                !_textCachedFontId.Equals(ComputedStyle.Font)
                || (!_textCachedNodeSize?.Equals(ComputedStyle.Size) ?? true)
                || (!_textCachedFontSize?.Equals(ComputedStyle.FontSize) ?? true)
                || (!_textCachedWordWrap?.Equals(ComputedStyle.WordWrap) ?? true)
                || (!_textCachedNodeValue?.Equals(_nodeValue) ?? true)
                || !_textCachedPadding.Equals(ComputedStyle.Padding)
            );
    }

    private string GetNormalizedString(string input)
    {
        return ComputedStyle.Font == 4 ? input : GameGlyphRegex().Replace(input, string.Empty);
    }

    [GeneratedRegex(@"[\uE020-\uE0DB]")]
    private static partial Regex GameGlyphRegex();
}
