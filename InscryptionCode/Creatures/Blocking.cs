using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Who takes an enemy's hit on the player. A creature in the attacker's lane takes it first; whatever gets past (its
/// overkill, or the whole hit when no creature can block) meets the player's Block, then HP.
/// The engine orders this the other way for pets (the owner's Block is spent before Osty takes anything), so for
/// a hit a creature will block, the player's Block is set aside until the creature has taken its share.
/// </summary>
internal static class Blocking
{
    // The hit being resolved, when a creature blocks it: its target (the player) and the blocker.
    private static (Creature Player, Creature Blocker)? _pending;

    public static Creature? PendingBlocker(Creature target) =>
        _pending is { } pending && pending.Player == target ? pending.Blocker : null;

    /// <summary>
    /// Before an enemy's powered attack on <paramref name="target"/> is blocked: choose the creature that takes it.
    /// Burrower digs into the attacked lane here.
    /// </summary>
    public static void BeforeHit(Player player, Creature target, ValueProp props, Creature? dealer)
    {
        _pending = null;
        if (target != player.Creature || dealer is not { IsEnemy: true } || !props.IsPoweredAttack() || !Board.UsesLanes(player))
        {
            return;
        }
        if (BlockerFor(player, dealer) is { } blocker)
        {
            _pending = (target, blocker);
        }
    }

    /// <summary>The creature that takes a hit from <paramref name="dealer"/>, or null if it reaches the player.</summary>
    private static Creature? BlockerFor(Player player, Creature dealer)
    {
        int lane = Board.LaneOf(dealer);
        if (lane < 0)
        {
            return null;
        }
        if (Board.CreatureInLane(player, lane) is { } inLane)
        {
            return CanBlock(inLane, dealer) ? inLane : null;
        }
        // Burrower: digs into the empty lane being attacked and takes the hit.
        foreach (var burrower in Board.Creatures(player).Where(c => Sigils.Has(c, Sigil.Burrower) && CanBlock(c, dealer)))
        {
            int from = Board.LaneOf(burrower);
            bool moved = Board.MoveTo(burrower, lane);
            MainFile.Logger.Info($"Burrower: {burrower.Monster?.Id.Entry} in lane {from + 1}, {dealer.Monster?.Id.Entry} attacks lane {lane + 1}: {(moved ? "moved to block" : "could not move")}");
            if (moved)
            {
                return burrower;
            }
        }
        return null;
    }

    /// <summary>Can this creature block the dealer's hit? Flyers pass over all but Mighty Leap; Waterborne submerges.</summary>
    private static bool CanBlock(Creature creature, Creature dealer) =>
        creature.IsAlive && !Sigils.Has(creature, Sigil.Waterborne)
        && (!Sigils.IsAirborneEnemy(dealer) || Sigils.Has(creature, Sigil.MightyLeap));

    /// <summary>
    /// Damage of a blocked hit now reaching the player (the creature's overkill): apply the Block set aside before.
    /// Returns what is left for HP.
    /// </summary>
    public static decimal AfterCreature(Creature target, decimal amount, ValueProp props)
    {
        if (PendingBlocker(target) == null)
        {
            return amount;
        }
        _pending = null;
        return amount - target.DamageBlockInternal(amount, props);
    }

    public static void Clear(Creature target)
    {
        if (PendingBlocker(target) != null)
        {
            _pending = null;
        }
    }

    // The engine spends the target's Block (a pet's owner's Block, for pets) before any redirect. Hold it back while
    // a creature is about to take the hit.
    [HarmonyPatch(typeof(Creature), nameof(Creature.DamageBlockInternal))]
    private static class HoldBlock
    {
        [HarmonyPrefix]
        private static bool Prefix(Creature __instance, ref decimal __result)
        {
            if (PendingBlocker(__instance) == null)
            {
                return true;
            }
            __result = 0m;
            return false;
        }
    }
}
