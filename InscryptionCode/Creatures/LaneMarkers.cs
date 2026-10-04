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

    private static readonly LabelSettings Style = new()
    {
        Font = ResourceLoader.Load("res://themes/kreon_bold_shared.tres") as Font,
        FontSize = 30,
        FontColor = new Color(1f, 0.9f, 0.6f),
        OutlineSize = 10,
        OutlineColor = Colors.Black,
    };

    private static (ICombatState State, Player Me)? LaneContext(NCombatRoom room)
    {
        var state = room.CreatureNodes.Select(n => n.Entity.CombatState).FirstOrDefault(s => s != null);
        var me = LocalContext.GetMe(state);
        return state != null && me != null && Board.UsesLanes(me) ? (state, me) : null;
    }

    public static void Refresh(NCombatRoom room)
    {
        if (LaneContext(room) is not { } context)
        {
            return;
        }
        var (state, me) = context;
        var creatures = Board.Creatures(me);
        var enemies = Board.Enemies(state);
        foreach (var node in room.CreatureNodes)
        {
            bool own = creatures.Contains(node.Entity);
            int lane = own || enemies.Contains(node.Entity) ? Board.LaneOf(node.Entity) : -1;
            SetBadge(node, lane, ownCreature: own);
        }
    }

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
        if (room == null || NTargetManager.Instance?.IsInSelection != false || LaneContext(room) is not { } context)
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
        if (Board.Creatures(me).Contains(hovered))
        {
            return Board.EnemyInLane(state, Board.LaneOf(hovered));
        }
        if (Board.Enemies(state).Contains(hovered))
        {
            return Board.CreatureInLane(me, Board.LaneOf(hovered)) ?? me.Creature;
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
