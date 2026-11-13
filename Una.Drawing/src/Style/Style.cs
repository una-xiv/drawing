using System.Linq;
using System.Text;
using System.Threading;

namespace Una.Drawing;

/// <summary>
/// Defines the properties that specify the presentation of an element.
/// </summary>
public sealed partial class Style
{
    /// <summary>
    /// The amount of time, in milliseconds, that a transition from one
    /// style to another should take. Use the <see cref="TransitionType"/>
    /// property to configure the easing function used for the transition.
    /// </summary>
    public uint? TransitionDuration { get => _transitionDuration; set => Set(ref _transitionDuration, value); }
    private uint? _transitionDuration;
    
    /// <summary>
    /// The type of transition to use when changing from one style to another.
    /// </summary>
    public TransitionType? TransitionType { get => _transitionType; set => Set(ref _transitionType, value); }
    private TransitionType? _transitionType;
    
    /// <summary>
    /// A class name to add to the node when the current transition is done.
    /// This effectively allows you to ping-pong between two classes to keep
    /// animations going.
    /// </summary>
    public string? TransitionAddClass { get => _transitionAddClass; set => Set(ref _transitionAddClass, value); }
    private string? _transitionAddClass;
    
    /// <summary>
    /// Removes the given class from this node when the transition is done.
    /// </summary>
    public string? TransitionRemoveClass { get => _transitionRemoveClass; set => Set(ref _transitionRemoveClass, value); }
    private string? _transitionRemoveClass;
    
    /// <summary>
    /// Specifies the opacity of the node. Must be a value between 0 and 1.
    /// </summary>
    public float? Opacity { get => _opacity; set => Set(ref _opacity, value); }
    private float? _opacity;

    /// <summary>
    /// Whether the node's texture should be antialiased. This applies to
    /// background images, icons, and borders. Text is always antialiased,
    /// regardless of this setting.
    /// </summary>
    public bool? IsAntialiased { get => _isAntialiased; set => Set(ref _isAntialiased, value); }
    private bool? _isAntialiased;

    /// <summary>
    /// Increases every time a property of this style is assigned a different
    /// value. Used to determine if a computed style needs to be regenerated.
    /// </summary>
    internal int Version => Volatile.Read(ref _version);

    private int _version;

    private void Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        Interlocked.Increment(ref _version);
    }

    private void SetColor(ref Color? field, Color? value)
    {
        if (Una.Drawing.Color.AreIdentical(field, value)) return;

        field = value;
        Interlocked.Increment(ref _version);
    }

    private void SetBorderColor(ref BorderColor? field, BorderColor? value)
    {
        if (field.HasValue == value.HasValue
            && (!field.HasValue
                || (Una.Drawing.Color.AreIdentical(field.Value.Top, value!.Value.Top)
                    && Una.Drawing.Color.AreIdentical(field.Value.Right, value.Value.Right)
                    && Una.Drawing.Color.AreIdentical(field.Value.Bottom, value.Value.Bottom)
                    && Una.Drawing.Color.AreIdentical(field.Value.Left, value.Value.Left)))) return;

        field = value;
        Interlocked.Increment(ref _version);
    }

    private void SetGradientColor(ref GradientColor? field, GradientColor? value)
    {
        if (field.HasValue == value.HasValue
            && (!field.HasValue
                || (field.Value.Type == value!.Value.Type
                    && Una.Drawing.Color.AreIdentical(field.Value.Color1, value.Value.Color1)
                    && Una.Drawing.Color.AreIdentical(field.Value.Color2, value.Value.Color2)))) return;

        field = value;
        Interlocked.Increment(ref _version);
    }

    public override string ToString()
    {
        Type type = GetType();
        
        var properties = type.GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true } && p.GetValue(this) != null)
            .Select(p => $"    {p.Name}: {p.GetValue(this)}")
            .ToList();

        StringBuilder sb = new();
        
        foreach (var property in properties) {
            sb.AppendLine(property);
        }
        
        return sb.ToString();
    }
}
