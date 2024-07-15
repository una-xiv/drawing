/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Linq;

namespace Una.Drawing;

public class Color(byte r, byte g, byte b, float a = 1.0f)
{
    /// <summary>
    /// Specifies the name of this color.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Represents the current version of the color theme.
    /// </summary>
    public static uint ThemeVersion { get; private set; } = 0;

    private static readonly Dictionary<string, uint> NamedColors = [];

    /// <summary>
    /// Returns the red component of this color as a byte.
    /// </summary>
    public byte R {
        set => r = value;
        get {
            if (Name != string.Empty && NamedColors.TryGetValue(Name, out uint color)) {
                return (byte)(color >> 16);
            }

            return r;
        }
    }

    /// <summary>
    /// Returns the green component of this color as a byte.
    /// </summary>
    public byte G {
        set => g = value;
        get {
            if (Name != string.Empty && NamedColors.TryGetValue(Name, out uint color)) {
                return (byte)(color >> 8);
            }

            return g;
        }
    }

    /// <summary>
    /// Returns the blue component of this color as a byte.
    /// </summary>
    public byte B {
        set => b = value;
        get {
            if (Name != string.Empty && NamedColors.TryGetValue(Name, out uint color)) {
                return (byte)(color);
            }

            return b;
        }
    }

    /// <summary>
    /// Returns the alpha component of this color as a float.
    /// </summary>
    public float A {
        set => a = value;
        get {
            if (Name != string.Empty && NamedColors.TryGetValue(Name, out uint color)) {
                return (color >> 24) / 255f;
            }

            return a;
        }
    }

    /// <summary>
    /// Assigns a named color to a specific color.
    /// </summary>
    public static void AssignByName(string name, uint color)
    {
        NamedColors[name] = color;
        ThemeVersion++;

        if (ThemeVersion >= uint.MaxValue - 1) {
            ThemeVersion = 1;
        }
    }

    /// <summary>
    /// Returns a list of all assigned color names.
    /// </summary>
    public static List<string> GetAssignedNames()
    {
        return NamedColors.Keys.ToList();
    }

    /// <summary>
    /// Returns the named color as a UInt32.
    /// </summary>
    public static uint GetNamedColor(string name)
    {
        return NamedColors.TryGetValue(name, out uint color) ? color : 0;
    }

    /// <summary>
    /// Returns a UInt32 representation of the color in ABGR format.
    /// </summary>
    /// <returns></returns>
    public uint ToUInt()
    {
        if (Name != string.Empty && NamedColors.TryGetValue(Name, out uint color)) {
            return color;
        }

        return (uint)((byte)(A * 255) << 24 | R << 16 | G << 8 | B);
    }

    public Color(byte b, byte g, byte r, byte a) : this(b, g, r, a * 255) { }

    public Color(uint color) : this(
        (byte)((color >> 16) & 0xFF),
        (byte)((color >> 8) & 0xFF),
        (byte)((color) & 0xFF),
        (byte)((color >> 24) & 0xFF)
    ) { }

    public Color(string name) : this(0, 0, 0, 0)
    {
        Name = name;
    }

    /// <summary>
    /// Returns true if this color is visible.
    /// </summary>
    public bool IsVisible => A > 0;

    public override string ToString()          => $"Color(#{ToUInt():x8})";
    public override bool   Equals(object? obj) => obj is Color color && color.ToUInt() == ToUInt();
    public override int    GetHashCode()       => ToUInt().GetHashCode();

    public static bool operator ==(Color? left, Color? right) => left is not null && left.Equals(right);
    public static bool operator !=(Color? left, Color? right) => !(left == right);

    internal static SKColor ToSkColor(Color? color)
    {
        return color is null
            ? SKColor.Empty
            : new(color.R, color.G, color.B, (byte)(color.A * 255));
    }
}
