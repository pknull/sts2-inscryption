using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption's board: four fixed lanes on each side. A creature takes the lowest empty lane when it arrives and
/// keeps it until it dies; the lane then stays empty until something new fills it. Enemies are laned the same way,
/// in the order the combat lists them; enemies beyond four have no lane (they hit the player) until one frees up.
/// In co-op every Luke has his own four lanes, facing the same enemy lanes.
/// Lanes are handed out by game logic only (these lookups, run by summons, combat start, arrivals and the rules),
/// which runs the same on every player's machine. The UI reads lanes through <see cref="Shown"/>, which hands out
/// none: it redraws on its own schedule on each machine, and a lane given out at a different moment would split the
/// players' games apart.
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
        var living = LivingCreatures(player);
        AssignLanes(living);
        return living.OrderBy(LaneOrNone).ToList();
    }

    public static List<Creature> Enemies(ICombatState? combatState)
    {
        var living = LivingEnemies(combatState);
        AssignLanes(living);
        return living;
    }

    private static List<Creature> LivingCreatures(Player player) =>
        player.PlayerCombatState?.Pets.Where(p => p.IsAlive && p.Monster is BoardCreature).ToList() ?? [];

    private static List<Creature> LivingEnemies(ICombatState? combatState) =>
        combatState?.Enemies.Where(e => e.IsAlive).ToList() ?? [];

    /// <summary>The same lookups for the UI: lanes as game logic last handed them out (-1 for none yet).</summary>
    public static class Shown
    {
        public static List<Creature> Creatures(Player player) =>
            LivingCreatures(player).OrderBy(LaneOrNone).ToList();

        public static List<Creature> Enemies(ICombatState? combatState) => LivingEnemies(combatState);

        public static int LaneOf(Creature creature) => LaneOrNone(creature);

        public static Creature? CreatureInLane(Player player, int lane) =>
            lane < 0 ? null : LivingCreatures(player).FirstOrDefault(c => LaneOrNone(c) == lane);

        public static Creature? EnemyInLane(ICombatState? combatState, int lane) =>
            lane < 0 ? null : LivingEnemies(combatState).FirstOrDefault(e => LaneOrNone(e) == lane);

        public static Creature? StrikeTarget(ICombatState? combatState, int lane) =>
            Nearest(LivingEnemies(combatState), lane);
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

    /// <summary>
    /// What a creature's strike into <paramref name="lane"/> hits: the enemy in that lane, or, if it is empty, the
    /// enemy nearest to it (the lower lane on a tie). Inscryption sends a strike into an empty lane through to the
    /// opponent's scale; here the enemies are the opponent (Keeper, 2026-10-06, reversing #7).
    /// </summary>
    public static Creature? StrikeTarget(ICombatState? combatState, int lane) => Nearest(Enemies(combatState), lane);

    private static Creature? Nearest(List<Creature> enemies, int lane)
    {
        if (lane < 0)
        {
            return null;
        }
        var laned = enemies.Where(e => LaneOrNone(e) >= 0).ToList();
        return laned.Count == 0
            ? enemies.FirstOrDefault()
            : laned.OrderBy(e => Math.Abs(LaneOrNone(e) - lane)).ThenBy(LaneOrNone).First();
    }

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

    /// <summary>Redraw the player's creatures' intents (their damage per strike changed).</summary>
    public static void RefreshIntents(Player? player)
    {
        if (player == null)
        {
            return;
        }
        foreach (var creature in Shown.Creatures(player))
        {
            (creature.Monster as BoardCreature)?.ShowIntent();
        }
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
