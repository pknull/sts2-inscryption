using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Makes the lanes visible: a numbered badge on each of your creatures and each enemy (your 1 blocks their 1),
/// and hovering either side of a lane puts the selection reticle on the other side. Hovering an enemy whose
/// lane is empty highlights the player, who takes that hit.
/// </summary>
internal static class LaneMarkers
{
    private const string BadgeName = "InscryptionLaneBadge";
    private static NCreature? _linked;

    private const string EmptySlotName = "InscryptionEmptySlot";

    private static readonly LabelSettings Style = new()
    {
        Font = ResourceLoader.Load("res://themes/kreon_bold_shared.tres") as Font,
        FontSize = 30,
        FontColor = new Color(1f, 0.9f, 0.6f),
        OutlineSize = 10,
        OutlineColor = Colors.Black,
    };

    // The lane a dragged creature card would land in: its empty-slot number at full brightness.
    private static readonly LabelSettings TargetStyle = new()
    {
        Font = Style.Font,
        FontSize = 38,
        FontColor = new Color(1f, 0.95f, 0.7f),
        OutlineSize = 12,
        OutlineColor = Colors.Black,
    };

    private static int _highlight = -1;
    private static readonly List<NCreature> Highlighted = [];

    // An empty lane's number, dimmed, where its creature would stand: shows where to drag a creature card.
    private static readonly LabelSettings EmptyStyle = new()
    {
        Font = Style.Font,
        FontSize = 30,
        FontColor = new Color(1f, 0.9f, 0.6f, 0.35f),
        OutlineSize = 10,
        OutlineColor = new Color(0f, 0f, 0f, 0.35f),
    };

    private static (ICombatState State, Player Me)? LaneContext(NCombatRoom room)
    {
        var state = room.CreatureNodes.Select(n => n.Entity.CombatState).FirstOrDefault(s => s != null);
        var me = LocalContext.GetMe(state);
        return state != null && me != null && Board.UsesLanes(me) ? (state, me) : null;
    }

    public static void Refresh(NCombatRoom room)
    {
        var state = room.CreatureNodes.Select(n => n.Entity.CombatState).FirstOrDefault(s => s != null);
        // Every Luke's creatures carry lane badges (co-op shows the other Luke's too); the enemies' are shared.
        if (state == null || !state.Players.Any(Board.UsesLanes))
        {
            return;
        }
        var creatures = state.Players.Where(Board.UsesLanes).SelectMany(Board.Shown.Creatures).ToList();
        var enemies = Board.Shown.Enemies(state);
        foreach (var node in room.CreatureNodes)
        {
            bool own = creatures.Contains(node.Entity);
            int lane = own || enemies.Contains(node.Entity) ? Board.Shown.LaneOf(node.Entity) : -1;
            SetBadge(node, lane, ownCreature: own);
        }
        if (LaneContext(room) is { } context)
        {
            MarkEmptySlots(room, context.Me, Board.Shown.Creatures(context.Me));
        }
    }

    private static void MarkEmptySlots(NCombatRoom room, Player me, List<Creature> creatures)
    {
        var ownerNode = room.GetCreatureNode(me.Creature);
        if (ownerNode?.GetParent() is not { } parent)
        {
            return;
        }
        for (int lane = 0; lane < Board.LaneCount; lane++)
        {
            if (BoardLayout.Slot(room, me, lane) is not { } slot)
            {
                continue;
            }
            var marker = parent.GetNodeOrNull<Label>(EmptySlotName + lane);
            if (marker == null)
            {
                marker = new Label
                {
                    Name = EmptySlotName + lane, Text = (lane + 1).ToString(), LabelSettings = EmptyStyle,
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                };
                parent.AddChild(marker);
            }
            // Same spot as an occupant's badge: above the top-left of the slot.
            marker.Position = slot + new Vector2(-BoardLayout.SlotWidth * 0.5f, -BoardLayout.SlotHeight - 36f);
            marker.Visible = !creatures.Any(c => Board.Shown.LaneOf(c) == lane);
        }
    }

    public static void Clear(NCreature node) => node.GetNodeOrNull<Label>(BadgeName)?.QueueFree();

    /// <summary>
    /// Light up <paramref name="lane"/> while a creature card is dragged there (-1 clears): the empty slot's number
    /// brightens, and the creature already there and the enemy facing the lane get the targeting reticle.
    /// </summary>
    public static void HighlightLane(int lane)
    {
        if (lane == _highlight)
        {
            return;
        }
        foreach (var node in Highlighted.Where(GodotObject.IsInstanceValid))
        {
            node.HideSingleSelectReticle();
        }
        Highlighted.Clear();
        var room = NCombatRoom.Instance;
        var context = room == null ? null : LaneContext(room);
        if (room != null && context is { } previous && SlotMarker(room, previous.Me, _highlight) is { } old)
        {
            old.LabelSettings = EmptyStyle;
        }
        _highlight = lane;
        if (lane < 0 || room == null || context is not { } current)
        {
            return;
        }
        var (state, me) = current;
        foreach (var creature in new[] { Board.Shown.CreatureInLane(me, lane), Board.Shown.EnemyInLane(state, lane) })
        {
            if (creature != null && room.GetCreatureNode(creature) is { } node)
            {
                node.ShowSingleSelectReticle();
                Highlighted.Add(node);
            }
        }
        if (SlotMarker(room, me, lane) is { } marker)
        {
            marker.LabelSettings = TargetStyle;
        }
    }

    private static Label? SlotMarker(NCombatRoom room, Player me, int lane) =>
        lane < 0 ? null : room.GetCreatureNode(me.Creature)?.GetParent()?.GetNodeOrNull<Label>(EmptySlotName + lane);

    private static void SetBadge(NCreature node, int lane, bool ownCreature)
    {
        var badge = node.GetNodeOrNull<Label>(BadgeName);
        if (lane < 0)
        {
            badge?.QueueFree();
            return;
        }
        if (badge == null)
        {
            badge = new Label { Name = BadgeName, LabelSettings = Style, MouseFilter = Control.MouseFilterEnum.Ignore };
            node.AddChild(badge);
        }
        badge.Text = (lane + 1).ToString();
        if (ownCreature)
        {
            // Above the top-left of the lane slot (the node's origin is at the creature's feet). Fixed, so it does
            // not drift while the creature scales into its slot.
            badge.Position = new Vector2(-BoardLayout.SlotWidth * 0.5f, -BoardLayout.SlotHeight - 36f);
        }
        else
        {
            // Top-left corner of the enemy's art, nudged outward.
            var bounds = node.Visuals.Bounds;
            badge.Position = node.Visuals.Position + bounds.Position * node.Visuals.Scale + new Vector2(-12f, -8f);
        }
    }

    private static void Focus(NCreature hovered)
    {
        var room = NCombatRoom.Instance;
        if (room == null || NTargetManager.Instance?.IsInSelection != false || LaneDrop.IsDragging
            || LaneContext(room) is not { } context)
        {
            return;
        }
        var (state, me) = context;
        Unfocus();
        var partner = Partner(hovered.Entity, state, me);
        _linked = partner == null ? null : room.GetCreatureNode(partner);
        _linked?.ShowSingleSelectReticle();
    }

    /// <summary>Who sits across the lane: your creature's enemy, or the enemy's blocker (you if the lane is empty).</summary>
    private static Creature? Partner(Creature hovered, ICombatState state, Player me)
    {
        if (Board.Shown.Creatures(me).Contains(hovered))
        {
            return Board.Shown.EnemyInLane(state, Board.Shown.LaneOf(hovered));
        }
        if (Board.Shown.Enemies(state).Contains(hovered))
        {
            return Board.Shown.CreatureInLane(me, Board.Shown.LaneOf(hovered)) ?? me.Creature;
        }
        return null;
    }

    private static void Unfocus()
    {
        _linked?.HideSingleSelectReticle();
        _linked = null;
    }

    [HarmonyPatch(typeof(NCreature), "OnFocus")]
    private static class OnFocusPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NCreature __instance) => Focus(__instance);
    }

    [HarmonyPatch(typeof(NCreature), "OnUnfocus")]
    private static class OnUnfocusPatch
    {
        [HarmonyPostfix]
        private static void Postfix() => Unfocus();
    }

    [HarmonyPatch(typeof(NCombatRoom), "OnCombatSetUp")]
    private static class OnCombatSetUpPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance) => Refresh(__instance);
    }
}
