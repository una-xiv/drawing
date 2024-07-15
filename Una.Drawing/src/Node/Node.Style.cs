/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Threading;

namespace Una.Drawing;

public partial class Node
{
    /// <summary>
    /// Signals the listener that a property that affects the layout of this
    /// node or any of its descendants been modified. This event can be
    /// triggered multiple times during a single reflow operation. It is up
    /// to the listener to keep track of the changes and perform the reflow
    /// operation as efficiently as possible.
    /// </summary>
    public Action? OnReflow;

    /// <summary>
    /// Defines the properties that specify the visual representation of this
    /// element.
    /// </summary>
    /// <remarks>
    /// Modifying this property directly will trigger a reflow. The same
    /// applies to modifications made to some of the properties of the given
    /// <see cref="Style"/> object.
    /// </remarks>
    /// <exception cref="ArgumentNullException"></exception>
    public Style Style {
        get => _style;
        set {
            _style = value ?? throw new ArgumentNullException(nameof(value));
            SignalReflow();
        }
    }

    public Stylesheet? Stylesheet {
        get => _stylesheet ?? ParentNode?.Stylesheet;
        set {
            _stylesheet = value;
            SignalReflow();
        }
    }

    /// <summary>
    /// Defines the final computed style of this node.
    /// </summary>
    public ComputedStyle ComputedStyle;

    private ComputedStyle _intermediateStyle;
    private Style         _style = new();
    private Stylesheet?   _stylesheet;
    private bool          _isUpdatingStyle;
    private bool          _hasComputedStyle;
    private int           _computeStyleLock = 0;

    private readonly object _lockObject = new();

    public static bool UseThreadedStyleComputation { get; set; } = false;

    /// <summary>
    /// Generates the computed style of this node and its descendants.
    /// </summary>
    private bool ComputeStyle()
    {
        if (_isUpdatingStyle) return false;

        _isUpdatingStyle = true;

        if (0 == Interlocked.Exchange(ref _computeStyleLock, 1)) {
            lock (_lockObject) {
                if (UseThreadedStyleComputation) {
                    lock (TagsList) {
                        InheritTagsFromParent();
                    }
                }

                var  style     = ComputedStyleFactory.Create(this);
                int  result    = style.Commit(ref _intermediateStyle);
                bool isUpdated = result > 0;

                _intermediateStyle = style;

                lock (_childNodes) {
                    foreach (Node child in _childNodes) {
                        if (child.ComputeStyle()) {
                            isUpdated = true;
                        }
                    }
                }

                if (isUpdated) {
                    ReassignAnchorNodes();
                }

                if (result is 1 or 3) SignalReflowRecursive();
                if (result is 2 or 3) SignalRepaint();

                ComputedStyle     = _intermediateStyle;
                _isUpdatingStyle  = false;
                _hasComputedStyle = true;


                // Release lock.
                Interlocked.Exchange(ref _computeStyleLock, 0);

                return isUpdated;
            }
        }

        return false;
    }

    /// <summary>
    /// Invokes the reflow event, signaling that the layout of this element
    /// or any of its descendants has changed.
    /// </summary>
    private void SignalReflow()
    {
        OnReflow?.Invoke();
        _mustReflow = true;
    }

    /// <summary>
    /// Performs a recursive reflow to all ancestor nodes.
    /// </summary>
    private void SignalReflowRecursive(bool signalParent = true)
    {
        _mustReflow = true;

        if (signalParent) ParentNode?.SignalReflowRecursive();

        if (_childNodes.Count > 0) {
            foreach (Node child in _childNodes) {
                child.SignalReflowRecursive(false);
            }
        }
    }

    /// <summary>
    /// Forces a repaint of the texture for this node on the next frame.
    /// </summary>
    private void SignalRepaint()
    {
        _texture?.Dispose();
        _texture = null;
    }
}
