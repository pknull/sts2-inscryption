using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Lays the player's creatures out as Inscryption's board: four fixed lane slots in a row in front of the player,
/// lane 1 nearest. Each creature is fitted inside its slot, so the row has the same footprint whatever is on it and
/// never depends on where the enemies stand; the player never moves.
/// <c>NCombatRoom.AddCreature</c> instead spreads pets across the owner's feet and makes them non-interactable,
/// which hides their HP bars; Osty is exempted from that the same way.
/// </summary>
internal static class BoardLayout
{
    public const float SlotWidth = 115f;
    public const float SlotHeight = 110f;
    private const float Gap = 10f;

    /// <summary>Lanes 2 and 4 stand this far back (up), so neighbouring health bars and statuses don't collide.</summary>
    public const float BackRowLift = 40f;

    private const string BaseScale = "inscryption_base_scale";

    private static void Arrange(NCombatRoom room, Player player)
    {
        var ownerNode = room.GetCreatureNode(player.Creature);
        if (ownerNode == null)
        {
            return;
        }
        foreach (var creature in Board.Creatures(player))
        {
            var node = room.GetCreatureNode(creature);
            if (node == null)
            {
                continue;
            }
            // SetScaleAndHue overwrites DefaultScale, so fit from the scale the creature arrived with.
            if (!node.HasMeta(BaseScale))
            {
                node.SetMeta(BaseScale, node.Visuals.DefaultScale);
            }
            float baseScale = node.GetMeta(BaseScale).AsSingle();
            float width = node.Visuals.Bounds.Size.X * baseScale;
            float height = node.Visuals.Bounds.Size.Y * baseScale;
            float fit = Mathf.Min(1f, Mathf.Min(SlotWidth / width, SlotHeight / height));

            // Each creature sits in its own lane's slot; empty lanes stay empty. Lanes 1 and 3 stand in front.
            int lane = Board.LaneOf(creature);
            node.Position = SlotLocal(ownerNode, lane);
            node.ZIndex = lane % 2 == 1 ? 0 : 1;
            // Unlike ScaleTo (a temporary visual scale), this also resizes the hitbox, reticle and health bar.
            node.SetScaleAndHue(baseScale * fit, 0f);
            node.ToggleIsInteractable(true);
        }
    }

    /// <summary>
    /// Where a creature in <paramref name="lane"/> stands (its feet), in the coordinates of the player's node's
    /// parent. Lanes 2 and 4 stand back (up) so neighbouring health bars don't collide.
    /// </summary>
    public static Vector2 SlotLocal(NCreature ownerNode, int lane)
    {
        float start = ownerNode.Position.X + ownerNode.Visuals.Bounds.Size.X * 0.5f + Gap * 2;
        float slotLeft = start + lane * (SlotWidth + Gap);
        return new Vector2(slotLeft + SlotWidth * 0.5f, ownerNode.Position.Y + 10f - (lane % 2 == 1 ? BackRowLift : 0f));
    }

    /// <summary>The same point in canvas coordinates, to compare with the mouse; null if the player has no node.</summary>
    public static Vector2? SlotGlobal(NCombatRoom room, Player player, int lane)
    {
        var ownerNode = room.GetCreatureNode(player.Creature);
        return ownerNode?.GetParent() is CanvasItem parent ? parent.GetGlobalTransform() * SlotLocal(ownerNode, lane) : null;
    }

    /// <summary>Re-run the layout, lane badges and attack intents (Leader changes neighbours' power).</summary>
    public static void Refresh()
    {
        if (NCombatRoom.Instance is { } room)
        {
            Rearrange(room);
        }
    }

    /// <summary>Re-run the layout and lane badges whenever any creature comes or goes.</summary>
    private static void Rearrange(NCombatRoom room)
    {
        // A creature being removed may already have left its combat state, so read it from the live nodes.
        var state = room.CreatureNodes.Select(n => n.Entity.CombatState).FirstOrDefault(s => s != null);
        var me = LocalContext.GetMe(state);
        if (me != null && Board.UsesLanes(me))
        {
            Arrange(room, me);
            foreach (var creature in Board.Creatures(me))
            {
                ((BoardCreature)creature.Monster!).ShowIntent();
            }
        }
        LaneMarkers.Refresh(room);
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
    private static class OnAdd
    {
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance) => Rearrange(__instance);
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.RemoveCreatureNode))]
    private static class OnRemove
    {
        // The node leaves the room's list at once but stays on screen for its death animation, which the badge
        // refresh no longer reaches; take its lane badge off here.
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance, NCreature node)
        {
            LaneMarkers.Clear(node);
            Rearrange(__instance);
        }
    }
}
