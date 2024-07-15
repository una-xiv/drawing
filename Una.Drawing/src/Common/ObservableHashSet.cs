/* Una.Drawing                                                 ____ ___
 *   A declarative drawing library for FFXIV.                 |    |   \____ _____        ____                _
 *                                                            |    |   /    \\__  \      |    \ ___ ___ _ _ _|_|___ ___
 * By Una. Licensed under AGPL-3.                             |    |  |   |  \/ __ \_    |  |  |  _| .'| | | | |   | . |
 * https://github.com/una-xiv/drawing                         |______/|___|  (____  / [] |____/|_| |__,|_____|_|_|_|_  |
 * ----------------------------------------------------------------------- \/ --- \/ ----------------------------- |__*/

namespace Una.Drawing;

public class ObservableHashSet<T> : HashSet<T>
{
    public event Action<T>? ItemAdded;
    public event Action<T>? ItemRemoved;

    public new void Add(T item)
    {
        base.Add(item);
        ItemAdded?.Invoke(item);
    }

    public new void Remove(T item)
    {
        if (!Contains(item)) return;

        base.Remove(item);
        ItemRemoved?.Invoke(item);
    }
}
