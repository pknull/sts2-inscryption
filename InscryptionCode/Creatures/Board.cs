using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption's board: four fixed lanes on each side. A creature takes the lowest empty lane when it arrives and
/// keeps it until it dies; the lane then stays empty until something new fills it. Enemies are laned the same way,
/// in the order the combat lists them; enemies beyond four have no lane (they hit the player) until one frees up.
/// </summary>
public static class Board
{
    public const int LaneCount = Balance.MaxCreatures;

    private static readonly ConditionalWeakTable<Creature, StrongBox<int>> Lanes = new();

    /// <summary>Lanes only exist for the Luke Carder character.</summary>
    public static bool UsesLanes(Player player) => player.Character is global::Inscryption.InscryptionCode.Character.Inscryption;

    /// <summary>The player's living creatures, in lane order.</summary>
    public static List<Creature> Creatures(Player player)
    {
        var living = player.PlayerCombatState?.Pets.Where(p => p.IsAlive && p.Monster is BoardCreature).ToList() ?? [];
        AssignLanes(living);
        return living.OrderBy(LaneOrNone).ToList();
    }

    public static List<Creature> Enemies(ICombatState? combatState)
    {
        var living = combatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];
        AssignLanes(living);
        return living;
    }

    /// <summary>The creature's lane (0-3), or -1 if it has none.</summary>
    public static int LaneOf(Creature creature)
    {
        if (creature.PetOwner != null)
        {
            Creatures(creature.PetOwner);
        }
        else
        {
            Enemies(creature.CombatState);
        }
        return LaneOrNone(creature);
    }

    public static Creature? CreatureInLane(Player player, int lane) =>
        lane < 0 ? null : Creatures(player).FirstOrDefault(c => LaneOrNone(c) == lane);

    public static Creature? EnemyInLane(ICombatState? combatState, int lane) =>
        lane < 0 ? null : Enemies(combatState).FirstOrDefault(e => LaneOrNone(e) == lane);

    /// <summary>Move one of the player's creatures to an empty lane (Sprinter, Guardian, Burrower).</summary>
    public static bool MoveTo(Creature creature, int lane)
    {
        if (creature.PetOwner == null || lane < 0 || lane >= LaneCount || CreatureInLane(creature.PetOwner, lane) != null)
        {
            return false;
        }
        Lanes.AddOrUpdate(creature, new StrongBox<int>(lane));
        BoardLayout.Refresh();
        return true;
    }

    private static int LaneOrNone(Creature creature) => Lanes.TryGetValue(creature, out var lane) ? lane.Value : -1;

    /// <summary>
    /// Give each laneless creature in <paramref name="living"/> the lowest lane no other living creature on its side
    /// holds. Lanes of the dead are free again. Runs on every lookup, so arrival order decides the lane.
    /// </summary>
    private static void AssignLanes(List<Creature> living)
    {
        var taken = living.Select(LaneOrNone).Where(l => l >= 0).ToHashSet();
        foreach (var creature in living.Where(c => LaneOrNone(c) < 0))
        {
            int lane = Enumerable.Range(0, LaneCount).FirstOrDefault(l => !taken.Contains(l), -1);
            if (lane < 0)
            {
                return;
            }
            Lanes.AddOrUpdate(creature, new StrongBox<int>(lane));
            taken.Add(lane);
        }
    }
}
