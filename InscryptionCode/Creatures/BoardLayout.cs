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

    private static void Arrange(NCombatRoom room, Player player)
    {
        var ownerNode = room.GetCreatureNode(player.Creature);
        if (ownerNode == null)
        {
            return;
        }
        float start = ownerNode.Position.X + ownerNode.Visuals.Bounds.Size.X * 0.5f + Gap * 2;
        foreach (var creature in Board.Creatures(player))
        {
            var node = room.GetCreatureNode(creature);
            if (node == null)
            {
                continue;
            }
            float width = node.Visuals.Bounds.Size.X * node.Visuals.DefaultScale;
            float height = node.Visuals.Bounds.Size.Y * node.Visuals.DefaultScale;
            float scale = Mathf.Min(1f, Mathf.Min(SlotWidth / width, SlotHeight / height));
            // Each creature sits in its own lane's slot; empty lanes stay empty.
            float slotLeft = start + Board.LaneOf(creature) * (SlotWidth + Gap);
            node.Position = new Vector2(slotLeft + SlotWidth * 0.5f, ownerNode.Position.Y + 10f);
            node.ScaleTo(scale, 0.2);
            node.ToggleIsInteractable(true);
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
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance) => Rearrange(__instance);
    }
}
