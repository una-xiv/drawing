namespace Una.Drawing;

/// <summary>
/// Defines the properties that specify the presentation of an element.
/// </summary>
public partial class Style
{
    /// <summary>
    /// Determines whether this node is visible.
    /// </summary>
    public bool? IsVisible { get => _isVisible; set => Set(ref _isVisible, value); }
    private bool? _isVisible;

    /// <summary>
    /// <para>
    /// The anchor defines the point of origin of the element.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Modifying this property will trigger a reflow of the layout. This is a
    /// computationally expensive operation and should be done sparingly.
    /// </remarks>
    public Anchor? Anchor { get => _anchor; set => Set(ref _anchor, value); }
    private Anchor? _anchor;

    /// <summary>
    /// Defines the flow direction of the node.
    /// </summary>
    public Flow? Flow { get => _flow; set => Set(ref _flow, value); }
    private Flow? _flow;
    
    /// <summary>
    /// Defines the render order of child elements.
    /// </summary>
    public FlowOrder? FlowOrder { get => _flowOrder; set => Set(ref _flowOrder, value); }
    private FlowOrder? _flowOrder;

    /// <summary>
    /// Defines the gap between nodes.
    /// </summary>
    public float? Gap { get => _gap; set => Set(ref _gap, value); }
    private float? _gap;
}
