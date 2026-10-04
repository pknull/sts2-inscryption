using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Lays the player's creatures out as Inscryption's lanes: a row in front of the player, lane 0 nearest.
/// <c>NCombatRoom.AddCreature</c> instead spreads pets across the owner's feet and makes them non-interactable,
/// which hides their HP bars; Osty is exempted from that the same way.
/// </summary>
internal static class BoardLayout
{
    private const float Gap = 24f;

    private static void Arrange(NCombatRoom room, Player player)
    {
        var ownerNode = room.GetCreatureNode(player.Creature);
        if (ownerNode == null)
        {
            return;
        }
        float x = ownerNode.Position.X + ownerNode.Visuals.Bounds.Size.X * 0.5f + Gap;
        foreach (var creature in Board.Creatures(player))
        {
            var node = room.GetCreatureNode(creature);
            if (node == null)
            {
                continue;
            }
            float width = node.Visuals.Bounds.Size.X;
            node.Position = new Vector2(x + width * 0.5f, ownerNode.Position.Y + 10f);
            node.ToggleIsInteractable(true);
            x += width + Gap;
        }
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
    private static class OnAdd
    {
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance, Creature creature)
        {
            if (creature.Monster is BoardCreature && creature.PetOwner != null)
            {
                Arrange(__instance, creature.PetOwner);
            }
        }
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.RemoveCreatureNode))]
    private static class OnRemove
    {
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance, NCreature node)
        {
            if (node.Entity.Monster is BoardCreature && node.Entity.PetOwner != null)
            {
                Arrange(__instance, node.Entity.PetOwner);
            }
        }
    }
}
