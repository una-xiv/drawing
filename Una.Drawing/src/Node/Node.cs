/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;

namespace Una.Drawing;

public partial class Node : IDisposable
{
    /// <summary>
    /// Defines the global scale factor across all nodes.
    /// </summary>
    public static float ScaleFactor { get; set; } = 1.0f;

    /// <summary>
    /// Whether the global scale factor should affect the borders of the nodes.
    /// This should typically be disabled when using a scale factor of less than 1
    /// to prevent 1px-borders from becoming invisible.
    /// </summary>
    public static bool ScaleAffectsBorders { get; set; } = true;

    /// <summary>
    /// Defines a unique identifier for the node.
    /// </summary>
    public string? Id {
        get => _id;
        set {
            if (_id == value) return;

            if (string.IsNullOrWhiteSpace(value)) {
                _id = null;
            } else if (!IdentifierNamingRule().Match(value).Success) {
                throw new ArgumentException(
                    $"The given ID \"{value}\" is invalid. Node IDs must match the regex \"{IdentifierNamingRule()}\"."
                );
            } else {
                _id = value;
            }

            OnPropertyChanged?.Invoke("Id", _id);
        }
    }

    private byte[] _seStringPayload = [];

    /// <summary>
    /// Defines the textual content of this node.
    /// </summary>
    public object? NodeValue {
        get => _nodeValue;
        set {
            switch (_nodeValue) {
                case null when value is null:
                    return;
                case string oldStr when value is string newStr: {
                    if (oldStr.Equals(newStr)) return;
                    break;
                }
            }

            switch (value) {
                case SeString when ReferenceEquals(value, _nodeValue):
                    return;
                case SeString seStr: {
                    byte[] payload = seStr.Encode();
                    if (_seStringPayload.SequenceEqual(payload)) return;
                    _seStringPayload = payload;
                    break;
                }
            }

            _nodeValue           = value;
            _textCachedNodeValue = null;
            _texture             = null;

            OnPropertyChanged?.Invoke("NodeValue", _nodeValue);
            SignalReflow();
        }
    }

    /// <summary>
    /// Defines a tooltip text for this node.
    /// </summary>
    /// <remarks>
    /// Defining a tooltip makes this node interactive. This means that some
    /// nodes may no longer be interacted with if this node overlaps them.
    /// </remarks>
    public string? Tooltip { get; set; }

    /// <summary>
    /// Returns a list of class names applied to this node.
    /// </summary>
    public ObservableHashSet<string> ClassList {
        get => _classList;
        set {
            if (_classList.SequenceEqual(value)) return;

            _classList.Clear();

            foreach (string v in value) _classList.Add(v);
            OnPropertyChanged?.Invoke("ClassList", _classList);
        }
    }

    /// <summary>
    /// Returns a list of tags applied to this node. Tags are used to denote
    /// certain characteristics of the node, that can be used for querying and
    /// styling purposes.
    /// </summary>
    /// <example>
    /// A node with ID "example" and tags "active" and "hovered" can be queried
    /// using the following query: `example:active:hovered`.
    /// </example>
    public ObservableHashSet<string> TagsList {
        get => _tagsList;
        set {
            if (_tagsList.SequenceEqual(value)) return;

            _tagsList.Clear();

            foreach (string v in value) _tagsList.Add(v);
            OnPropertyChanged?.Invoke("TagsList", _tagsList);
        }
    }

    /// <summary>
    /// A list of child nodes of this node.
    /// </summary>
    public ObservableCollection<Node> ChildNodes {
        get => _childNodes;
        set {
            if (_childNodes.SequenceEqual(value)) return;

            List<Node> toRemove = _childNodes.ToList();

            foreach (var node in toRemove) node.Remove();
            foreach (var node in value) AppendChild(node);

            OnPropertyChanged?.Invoke("ChildNodes", _childNodes);
        }
    }

    /// <summary>
    /// A reference to the parent node of this node.
    /// </summary>
    public Node? ParentNode { get; private set; }

    /// <summary>
    /// Defines the sort index of this node. Nodes are sorted in ascending
    /// order in the parent's child nodes list based on this value.
    /// </summary>
    public int SortIndex {
        get => _sortIndex;
        set {
            if (_sortIndex == value) return;

            _sortIndex = value;
            OnSortIndexChanged?.Invoke();
            OnPropertyChanged?.Invoke("SortIndex", _childNodes);
        }
    }

    /// <summary>
    /// A reference to the root node of this node. Returns itself if the node
    /// where this property is accessed from has no parent node.
    /// </summary>
    public Node RootNode => ParentNode?.RootNode ?? this;

    /// <summary>
    /// A reference to the node immediately preceding this node in the parent's
    /// child nodes list. Returns null if this node has no parent node, or if
    /// this node is the first child node of its parent.
    /// </summary>
    public Node? PreviousSibling => ParentNode?.ChildNodes.ElementAtOrDefault(ParentNode.ChildNodes.IndexOf(this) - 1);

    /// <summary>
    /// A reference to the node immediately following this node in the parent's
    /// child nodes list. Returns null if this node has no parent node, or if
    /// this node is the last child node of its parent.
    /// </summary>
    public Node? NextSibling => ParentNode?.ChildNodes.ElementAtOrDefault(ParentNode.ChildNodes.IndexOf(this) + 1);

    /// <summary>
    /// Invoked when one of the node's properties have been modified.
    /// </summary>
    public event Action<string, object?>? OnPropertyChanged;

    /// <summary>
    /// Invoked when a new child node has been added to this node.
    /// </summary>
    public event Action<Node>? OnChildAdded;

    /// <summary>
    /// Invoked when a child node has been removed from this node.
    /// </summary>
    public event Action<Node>? OnChildRemoved;

    /// <summary>
    /// Invoked when a class name has been added to the class list.
    /// </summary>
    public event Action<string>? OnClassAdded;

    /// <summary>
    /// Invoked when a class name has been removed from the class list.
    /// </summary>
    public event Action<string>? OnClassRemoved;

    /// <summary>
    /// Invoked when a tag has been added to the tags list.
    /// </summary>
    public event Action<string>? OnTagAdded;

    /// <summary>
    /// Invoked when a tag has been removed from the tags list.
    /// </summary>
    public event Action<string>? OnTagRemoved;

    /// <summary>
    /// Invoked when the sort index of this node has been changed.
    /// </summary>
    public event Action? OnSortIndexChanged;

    private string? _id;
    private object? _nodeValue;
    private int     _sortIndex;

    private readonly ObservableHashSet<string>  _classList  = [];
    private readonly ObservableHashSet<string>  _tagsList   = [];
    private          ObservableCollection<Node> _childNodes = [];

    public Node()
    {
        ComputedStyle = ComputedStyleFactory.CreateDefault();
        ComputedStyle.LayoutStyleSnapshot = new();
        ComputedStyle.PaintStyleSnapshot  = new();

        _childNodes.CollectionChanged += HandleChildListChanged;

        _classList.ItemAdded += c => {
            OnClassAdded?.Invoke(c);
            SignalReflow();
        };

        _classList.ItemRemoved += c => {
            OnClassRemoved?.Invoke(c);
            SignalReflow();
        };

        _tagsList.ItemAdded += t => {
            OnTagAdded?.Invoke(t);
            SignalReflow();
        };

        _tagsList.ItemRemoved += t => {
            OnTagRemoved?.Invoke(t);
            SignalReflow();
        };

        OnChildAdded   += child => child.OnReflow += SignalReflow;
        OnChildRemoved += child => child.OnReflow -= SignalReflow;

        FontRegistry.FontChanged += OnFontConfigurationChanged;
    }

    public void Dispose()
    {
        foreach (var child in _childNodes) child.Dispose();

        // TODO: Cancel compute style token.

        FontRegistry.FontChanged -= OnFontConfigurationChanged;

        GC.SuppressFinalize(this);
    }

    private void OnFontConfigurationChanged()
    {
        _texture             = null;
        _textCachedFontId    = null;
        _textCachedFontSize  = null;
        _textCachedNodeSize  = null;
        _textCachedWordWrap  = null;
        _textCachedNodeValue = null;
        _mustReflow          = true;
        _snapshot            = new();
    }

    private void HandleChildListChanged(object? _, NotifyCollectionChangedEventArgs e)
    {
        switch (e) {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }: {
                foreach (Node node in e.NewItems) OnChildAddedToList(node);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }: {
                foreach (Node node in e.OldItems) OnChildRemovedFromList(node);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Replace, OldItems: not null, NewItems: not null }: {
                foreach (Node node in e.OldItems!) OnChildRemovedFromList(node);
                foreach (Node node in e.NewItems!) OnChildAddedToList(node);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Reset, OldItems: not null }: {
                foreach (Node node in e.OldItems) OnChildRemovedFromList(node);
                break;
            }
        }

        SignalReflow();
        ReassignAnchorNodes();
    }

    private void HandleClassListChanged(object? _, NotifyCollectionChangedEventArgs e)
    {
        switch (e) {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }: {
                foreach (string className in e.NewItems) OnClassAdded?.Invoke(className);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }: {
                foreach (string className in e.OldItems) OnClassRemoved?.Invoke(className);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Replace, OldItems: not null, NewItems: not null }: {
                foreach (string className in e.NewItems) OnClassAdded?.Invoke(className);
                foreach (string className in e.OldItems) OnClassRemoved?.Invoke(className);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Reset, OldItems: not null }: {
                foreach (string className in e.OldItems) OnClassRemoved?.Invoke(className);
                break;
            }
        }

        SignalReflow();
    }

    private void HandleTagsListChanged(object? _, NotifyCollectionChangedEventArgs e)
    {
        switch (e) {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }: {
                foreach (string tag in e.NewItems) OnTagAdded?.Invoke(tag);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }: {
                foreach (string tag in e.OldItems) OnTagRemoved?.Invoke(tag);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Replace, OldItems: not null, NewItems: not null }: {
                foreach (string tag in e.NewItems) OnTagAdded?.Invoke(tag);
                foreach (string tag in e.OldItems) OnTagRemoved?.Invoke(tag);
                break;
            }
            case { Action: NotifyCollectionChangedAction.Reset, OldItems: not null }: {
                foreach (string tag in e.OldItems) OnTagRemoved?.Invoke(tag);
                break;
            }
        }

        SignalReflow();
    }

    /// <summary>
    /// Appends the given node to the child list of this node. Does nothing if
    /// the given node is already a child of this node.
    /// </summary>
    /// <remarks>
    /// Removes the node from its existing parent if it has one.
    /// </remarks>
    /// <param name="node">The node to append.</param>
    public void AppendChild(Node node)
    {
        lock (_childNodes) {
            if (_childNodes.Contains(node)) return;

            node.ParentNode?.RemoveChild(this);

            _childNodes.Add(node);
            node.ParentNode = this;
        }
    }

    /// <summary>
    /// Removes this node from its parent. Does nothing if this node has no
    /// parent node.
    /// </summary>
    public void Remove()
    {
        ParentNode?.RemoveChild(this);
    }

    /// <summary>
    /// Removes the given child node from this node. Does nothing if the given
    /// node is not a child of this node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveChild(Node node)
    {
        lock (_childNodes) {
            if (!_childNodes.Contains(node)) return;

            _childNodes.Remove(node);
        }
    }

    /// <summary>
    /// Replaces the given old node with the new node. Does nothing if the old
    /// node is not present in the child nodes list.
    /// </summary>
    /// <remarks>
    /// If the new node is already placed somewhere else, it is removed from
    /// that position prior to being added to this node.
    /// </remarks>
    /// <param name="oldChild">A reference to the old node.</param>
    /// <param name="newChild">A reference to the new node.</param>
    public void ReplaceChild(Node oldChild, Node newChild)
    {
        if (!_childNodes.Contains(oldChild)) return;

        ClearQuerySelectorCache();

        // Remove the new node from its parent if it has one.
        newChild.ParentNode?.RemoveChild(newChild);

        int index = _childNodes.IndexOf(oldChild);

        _childNodes[index]  = newChild;
        oldChild.ParentNode = null;
        newChild.ParentNode = this;

        OnChildRemoved?.Invoke(oldChild);
        OnChildAdded?.Invoke(newChild);
    }

    /// <summary>
    /// Invoked when a child node has been added to the child nodes list.
    /// </summary>
    /// <param name="node">The added node.</param>
    private void OnChildAddedToList(Node node)
    {
        ClearQuerySelectorCache();

        node.ParentNode?.RemoveChild(node);
        node.ParentNode = this;

        if (false == _anchorToChildNodes.ContainsKey(node.ComputedStyle.Anchor.Point)) {
            _anchorToChildNodes[node.ComputedStyle.Anchor.Point] = [];
        }

        _anchorToChildNodes[node.ComputedStyle.Anchor.Point].Add(node);
        _childNodeToAnchor[node] = node.ComputedStyle.Anchor.Point;

        node.OnSortIndexChanged += SortChildren;

        SortChildren();
        SignalReflow();
        OnChildAdded?.Invoke(node);
    }

    /// <summary>
    /// Invoked when a child node has been removed from the child nodes list.
    /// </summary>
    /// <param name="node">The removed node.</param>
    private void OnChildRemovedFromList(Node node)
    {
        ClearQuerySelectorCache();

        node.ParentNode         =  null;
        node.OnSortIndexChanged -= SortChildren;

        if (!_childNodeToAnchor.ContainsKey(node)) {
            return;
        }

        if (_childNodeToAnchor.Remove(node, out var anchor)) {
            _anchorToChildNodes[anchor].Remove(node);
        }

        SortChildren();
        OnChildRemoved?.Invoke(node);
    }

    private void SortChildren()
    {
        if (_childNodes.Count < 2) return;

        // Stable sort based on SortIndex. This code is only executed when
        // nodes are added or removed from the node list, or when the
        // SortIndex of a child node has been changed. This is a relatively
        // rare operation that does not happen on every frame.
        _childNodes.CollectionChanged -= HandleChildListChanged;
        _childNodes                   =  new(_childNodes.OrderBy(n => n.SortIndex));
        _childNodes.CollectionChanged += HandleChildListChanged;

        foreach ((Anchor.AnchorPoint pt, List<Node> nodes) in _anchorToChildNodes) {
            _anchorToChildNodes[pt] = [..nodes.OrderBy(n => n.SortIndex)];
        }

        SignalReflow();
    }

    private float _scaleFactor         = ScaleFactor;
    private bool  _scaleAffectsBorders = ScaleAffectsBorders;

    [GeneratedRegex("^[A-Za-z]{1}[A-Za-z0-9_-]+$")]
    private static partial Regex IdentifierNamingRule();
}
