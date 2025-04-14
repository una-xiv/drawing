﻿using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Una.Drawing;

public partial class Node
{
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

    /// <summary>
    /// The stylesheet that defines the styles for this node and its children.
    /// </summary>
    public Stylesheet? Stylesheet {
        get => _stylesheet ?? ParentNode?.Stylesheet;
        set {
            _stylesheet = value;
            SignalReflow();
        }
    }

    /// <summary>
    /// A lookup table of query selectors of which the match result has been
    /// performed and cached. This is used to avoid re-evaluating the same
    /// query selector multiple times.
    /// </summary>
    /// <remarks>
    /// This dictionary should be cleared on reflow to avoid stale results.
    /// </remarks>
    internal readonly Dictionary<QuerySelector, bool> CachedQuerySelectorResults = [];

    /// <summary>
    /// Defines the final computed style of this node.
    /// </summary>
    public ComputedStyle ComputedStyle = ComputedStyleFactory.CreateDefault();

    private ComputedStyle _intermediateStyle;
    private Style         _style = new();
    private Stylesheet?   _stylesheet;
    private bool          _isUpdatingStyle;
    private int           _computeStyleLock;
    private Animation?    _animation;
    private int           _lastStyleHash;
    private int           _lastNodeValueHash;

    private readonly Lock _lockObject = new();

    public static bool UseThreadedStyleComputation { get; set; }

    /// <summary>
    /// Generates the computed style of this node and its descendants.
    /// </summary>
    private bool ComputeStyle()
    {
        if (IsDisposed || _isUpdatingStyle) return false;

        _isUpdatingStyle = true;

        if (0 != Interlocked.Exchange(ref _computeStyleLock, 1)) {
            return false;
        }

        lock (_lockObject) {
            if (IsDisposed) return false;

            (int hash, ComputedStyle style) = ComputedStyleFactory.Create(this);
            ComputedStyle.CommitResult result = style.Commit(ref _intermediateStyle);

            if (_lastStyleHash != hash) {
                _lastStyleHash = hash;

                if (_animation is { IsPlaying: true }) {
                    _animation = null;
                }

                _animation ??= _intermediateStyle.TransitionDuration > 0
                    ? new Animation(_intermediateStyle, style)
                    : null;
            }

            if (_animation is { IsPlaying: true }) {
                style = _animation.Update(DrawDeltaTime);
            } else {
                _animation = null;
            }

            if (style.TransitionAddClass != null) {
                ToggleClass(style.TransitionAddClass, true);
            }

            if (style.TransitionRemoveClass != null) {
                ToggleClass(style.TransitionRemoveClass, false);
            }

            int  nodeValueHash   = NodeValue?.GetHashCode() ?? 0;
            bool isLayoutUpdated = nodeValueHash != _lastNodeValueHash;

            lock (_childNodes) {
                foreach (Node child in _childNodes.ToImmutableArray()) {
                    if (child.IsDisposed) continue;

                    if (child.ComputeStyle()) {
                        result          |= ComputedStyle.CommitResult.LayoutUpdated;
                        isLayoutUpdated =  true;
                    }
                }
            }

            // Update snapshot.
            _intermediateStyle                     = style;
            _intermediateStyle.LayoutStyleSnapshot = LayoutStyleSnapshot.Create(ref _intermediateStyle);
            _intermediateStyle.PaintStyleSnapshot  = PaintStyleSnapshot.Create(ref _intermediateStyle);
            ComputedStyle                          = _intermediateStyle;
            RenderHash                             = _intermediateStyle.GetHash();
            _isUpdatingStyle                       = false;

            if (result.HasFlag(ComputedStyle.CommitResult.LayoutUpdated)) {
                SignalReflow();
                ReassignAnchorNodes();
                isLayoutUpdated = true;
            }

            if (_previousRenderHash != RenderHash || _lastNodeValueHash != nodeValueHash) {
                _mustRepaint = true;
            }

            // Release lock.
            Interlocked.Exchange(ref _computeStyleLock, 0);

            _lastNodeValueHash = nodeValueHash;

            return isLayoutUpdated;
        }
    }

    /// <summary>
    /// Invokes the reflow event, signaling that the layout of this element
    /// or any of its descendants has changed.
    /// </summary>
    private void SignalReflow()
    {
        RootNode._mustReflow = true;
    }

    private void ClearCachedQuerySelectorsRecursively()
    {
        CachedQuerySelectorResults.Clear();

        lock (_childNodes) {
            foreach (var node in _childNodes) {
                node.ClearCachedQuerySelectorsRecursively();
            }
        }
    }

    private void ClearCachedQuerySelectors()
    {
        RootNode.ClearCachedQuerySelectorsRecursively();
    }
}