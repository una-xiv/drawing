using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace Una.Drawing.Clipping;

public static class ClipRectProvider
{
    private static readonly List<ClipRect> RectList = [];

    internal static unsafe void UpdateRects()
    {
        RectList.Clear();

        ForEachAtkUnit(
            (uPtr, _) => {
                var unitBase   = (AtkUnitBase*)uPtr;
                var rootNode   = unitBase->RootNode;
                var windowNode = unitBase->WindowNode;

                if (rootNode == null || windowNode == null) return;
                if (!rootNode->IsVisible()) return;

                Vector2 scale = new(rootNode->ScaleX, rootNode->ScaleY);

                // Main window rect.
                ClipRect windowRect = GetWindowClipRect(windowNode, scale);
                if (windowRect.IsValid()) RectList.Add(windowRect);

                // Find remaining components that need to be rendered.
                for (var i = 0; i < unitBase->UldManager.NodeListSize; i++) {
                    var node = unitBase->UldManager.NodeList[i];
                    if (node == null || !node->IsVisible()) continue;

                    Vector2  pos      = new(node->ScreenX, node->ScreenY);
                    Vector2  size     = new Vector2(node->Width, node->Height) * scale;
                    ClipRect clipRect = new(pos, pos + size);

                    if (windowRect.IsValid() &&
                        (windowRect.Overlaps(clipRect) || clipRect.Overlaps(windowRect))) continue;

                    if (!clipRect.IsValid()) continue;

                    RectList.Add(clipRect);
                }
            }
        );
    }

    private static unsafe ClipRect GetWindowClipRect(AtkComponentNode* componentNode, Vector2 scale)
    {
        var component = componentNode->Component;
        if (component == null) return new ClipRect();

        for (var i = 0; i < component->UldManager.NodeListSize; i++) {
            var node = component->UldManager.NodeList[i];
            if (node == null) continue;
            if (node->Type != NodeType.Image || !node->IsVisible()) continue;

            Vector2  size     = new Vector2(node->Width, node->Height) * scale;
            Vector2  topLeft  = new(node->ScreenX, node->ScreenY);
            ClipRect clipRect = new ClipRect(topLeft, topLeft + size);

            if (clipRect.IsValid()) {
                return clipRect;
            }
        }

        return new ClipRect();
    }

    public static List<ClipRect> FindClipRectsIntersectingWith(ClipRect clipRect)
    {
        return RectList.Where(other => other.IntersectsWith(clipRect)).ToList();
    }

    /// <summary>
    /// Returns the AtkUnitManager instance.
    /// </summary>
    /// <returns></returns>
    private static unsafe AtkUnitManager* GetAtkUnitManager()
    {
        AtkStage* stage = AtkStage.Instance();

        if (stage == null) {
            return null;
        }

        RaptureAtkUnitManager* manager = stage->RaptureAtkUnitManager;

        if (manager == null) {
            return null;
        }

        return &manager->AtkUnitManager;
    }

    /// <summary>
    /// Iterates over all visible ATK units and calls the given iterator for each.
    /// </summary>
    /// <param name="iterator"></param>
    /// <param name="focusedOnly"></param>
    private static unsafe void ForEachAtkUnit(Action<IntPtr, string> iterator, bool focusedOnly = false)
    {
        var unitManager = GetAtkUnitManager();
        var unitList    = focusedOnly ? unitManager->FocusedUnitsList : unitManager->AllLoadedUnitsList;

        for (var i = 0; i < unitList.Count; i++) {
            AtkUnitBase* unit = *(AtkUnitBase**)Unsafe.AsPointer(ref unitList.Entries[i]);

            if (null == unit || !unit->IsVisible || null == unit->WindowNode) continue;

            string? name = unit->NameString;
            if (name is "_FocusTargetInfo" or "JobHudNotice") continue;

            iterator((IntPtr)unit, name ?? string.Empty);
        }
    }
}