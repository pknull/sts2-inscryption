using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption's lanes, mapped onto Slay the Spire 2. A creature's lane is its position in summon order among the
/// player's living creatures; an enemy's lane is its position among living enemies. Creatures that die shift the
/// ones after them down a lane, unlike Inscryption's fixed slots.
/// </summary>
public static class Board
{
    public static List<Creature> Creatures(Player player) =>
        player.PlayerCombatState?.Pets.Where(p => p.IsAlive && p.Monster is BoardCreature).ToList() ?? [];

    public static int LaneOf(Creature creature) =>
        creature.PetOwner == null ? -1 : Creatures(creature.PetOwner).IndexOf(creature);

    public static List<Creature> Enemies(ICombatState? combatState) =>
        combatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];
}
