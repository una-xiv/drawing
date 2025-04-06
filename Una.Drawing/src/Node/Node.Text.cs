/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

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
    private int?     _textCachedMaxWidth;

    internal MeasuredText? NodeValueMeasurement { get; private set; }

    internal void ClearTextCache()
    {
        _textCachedPadding   = new();
        _textCachedNodeValue = null;
        _textCachedFontId    = null;
        _textCachedFontSize  = null;
        _textCachedWordWrap  = null;
        _textCachedNodeSize  = null;
        _textCachedMaxWidth  = null;
        NodeValueMeasurement = null;
    }

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
    internal Size ComputeContentSizeFromText()
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

        _texture?.Dispose();
        _texture = null;

        _textCachedNodeValue = _nodeValue;
        _textCachedWordWrap  = ComputedStyle.WordWrap;
        _textCachedPadding   = ComputedStyle.Padding.Copy();
        _textCachedFontId    = ComputedStyle.Font;
        _textCachedFontSize  = ComputedStyle.FontSize;
        _textCachedNodeSize  = ComputedStyle.Size.Copy();
        _textCachedMaxWidth  = ComputedStyle.MaxWidth;

        var font         = FontRegistry.Fonts[ComputedStyle.Font];
        var maxLineWidth = Math.Max(0, ComputedStyle.Size.Width);

        if (maxLineWidth == 0 && ComputedStyle.AutoSize.Horizontal == AutoSize.Grow) {
            maxLineWidth = Math.Max(0, Bounds.ContentSize.Width);
        }

        NodeValueMeasurement = font.MeasureText(
            str,
            ComputedStyle.FontSize,
            ComputedStyle.OutlineSize,
            maxLineWidth,
            ComputedStyle.WordWrap,
            ComputedStyle.TextOverflow,
            ComputedStyle.LineHeight,
            ComputedStyle.MaxWidth
        );

        return NodeValueMeasurement.Value.Size;
    }

    private Size ComputeContentSizeFromSeString()
    {
        if (_nodeValue is not SeString str || str.Payloads.Count == 0) {
            return new(0, 0);
        }

        if (false == MustRecomputeNodeValue()) {
            return NodeValueMeasurement?.Size ?? new();
        }

        IFont font       = FontRegistry.Fonts[ComputedStyle.Font];
        float maxWidth   = 0;
        float maxHeight  = 0;
        Size  charSize   = font.MeasureText("X", ComputedStyle.FontSize, ComputedStyle.OutlineSize).Size;
        float spaceWidth = charSize.Width;

        foreach (var payload in str.Payloads) {
            switch (payload) {
                case TextPayload text:
                    if (string.IsNullOrEmpty(text.Text)) continue;

                    MeasuredText measurement = font.MeasureText(
                        text.Text,
                        ComputedStyle.FontSize,
                        ComputedStyle.OutlineSize
                    );

                    if (ComputedStyle.MaxWidth > 0 && maxWidth + measurement.Size.Width > ComputedStyle.MaxWidth) {
                        measurement = font.MeasureText(
                            text.Text,
                            ComputedStyle.FontSize,
                            ComputedStyle.OutlineSize,
                            null,
                            false,
                            false,
                            maxWidth: ComputedStyle.MaxWidth - maxWidth
                        );
                    }

                    maxWidth  += measurement.Size.Width;
                    maxHeight =  Math.Max(maxHeight, measurement.Size.Height);
                    continue;
                case IconPayload:
                    maxWidth += spaceWidth + 20 + spaceWidth;
                    continue;
            }
        }

        _textCachedNodeValue = _nodeValue;
        _textCachedWordWrap  = ComputedStyle.WordWrap;
        _textCachedPadding   = ComputedStyle.Padding.Copy();
        _textCachedFontId    = ComputedStyle.Font;
        _textCachedFontSize  = ComputedStyle.FontSize;
        _textCachedNodeSize  = ComputedStyle.Size.Copy();
        _textCachedMaxWidth  = ComputedStyle.MaxWidth;

        NodeValueMeasurement = new() { Lines = [], LineCount = 1, Size = new(maxWidth, maxHeight) };

        return new(maxWidth, maxHeight);
    }

    /// <summary>
    /// Returns true if the computed text content and bounding box based on the
    /// node value must be recomputed.
    /// </summary>
    internal bool MustRecomputeNodeValue()
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
                   || (!_textCachedMaxWidth?.Equals(ComputedStyle.MaxWidth) ?? true)
                   || !_textCachedPadding.Equals(ComputedStyle.Padding)
               );
    }
}