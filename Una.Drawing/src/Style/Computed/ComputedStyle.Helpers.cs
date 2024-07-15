/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public partial struct ComputedStyle
{
    internal bool HasDrawables()
    {
        return BackgroundColor is not null
            || BorderColor is not null
            || StrokeColor is not null
            || BackgroundGradient is not null
            || BackgroundImage is not null
            || OutlineColor is not null
            || TextShadowColor is not null
            || IconId is not null
            || ImageBytes is not null
            || (!string.IsNullOrWhiteSpace(UldResource) && UldPartsId.HasValue && UldPartId.HasValue);
    }

    internal int Commit(ref ComputedStyle previous)
    {
        PaintStyleSnapshot  = PaintStyleSnapshot.Create(ref this);
        LayoutStyleSnapshot = LayoutStyleSnapshot.Create(ref this);

        var result = 0;

        if (!AreStructsEqual(ref LayoutStyleSnapshot, ref previous.LayoutStyleSnapshot)) {
            result = 1;
        }

        if (!AreStructsEqual(ref PaintStyleSnapshot, ref previous.PaintStyleSnapshot)) {
            result += 2;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool AreStructsEqual<T>(ref readonly T a, ref readonly T b) where T : unmanaged
    {
        return MemoryMarshal
            .AsBytes(new ReadOnlySpan<T>(in a))
            .SequenceEqual(MemoryMarshal.AsBytes(new ReadOnlySpan<T>(in b)));
    }
}
