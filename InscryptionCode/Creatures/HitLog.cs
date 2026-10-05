using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// One log line per hit on Luke or his creatures, so a playtest's damage can be split afterwards: who hit from which
/// lane, which creature took it and what it lost, and what reached Luke's Block and HP.
/// </summary>
internal static class HitLog
{
    private sealed class Hit
    {
        public required Player Player;
        public required Creature Target;
        public Creature? Dealer;
        public decimal Amount;
        public ValueProp Props;
        public int Lane;
        public int Block;
        public int Hp;
        public Creature? Blocker;
        public int BlockerHp;
        public DamageResult? BlockerResult;
    }

    // Keyed by target: one damage call hits all its targets before any result is reported.
    private static readonly Dictionary<Creature, Hit> Pending = new();

    public static void Reset() => Pending.Clear();

    /// <summary>Call after <see cref="Blocking.BeforeHit"/>, so the blocker it chose is known.</summary>
    public static void Before(Player player, Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        Pending.Remove(target);
        if (!Board.UsesLanes(player) || (target != player.Creature && target.PetOwner != player))
        {
            return;
        }
        var blocker = Blocking.PendingBlocker(target);
        Pending[target] = new Hit
        {
            Player = player,
            Target = target,
            Dealer = dealer,
            Amount = amount,
            Props = props,
            Lane = dealer is { IsEnemy: true } ? Board.LaneOf(dealer) : -1,
            // A pet's hits spend its owner's Block.
            Block = (target.PetOwner?.Creature ?? target).Block,
            Hp = target.CurrentHp,
            Blocker = blocker,
            BlockerHp = blocker?.CurrentHp ?? 0,
        };
    }

    public static void After(DamageResult result)
    {
        var receiver = result.Receiver;
        if (Pending.TryGetValue(receiver, out var hit))
        {
            Pending.Remove(receiver);
            MainFile.Logger.Info(Describe(hit, result));
            return;
        }
        foreach (var pending in Pending.Values)
        {
            if (pending.Blocker == receiver)
            {
                pending.BlockerResult = result;
                return;
            }
        }
    }

    private static string Describe(Hit hit, DamageResult result)
    {
        var target = hit.Target;
        string turn = target.CombatState?.RoundNumber.ToString() ?? "?";
        string from = hit.Dealer == null ? "no dealer" : Name(hit.Dealer) + (hit.Lane >= 0 ? $" in lane {hit.Lane + 1}" : "");
        string kind = hit.Props.IsPoweredAttack() ? "" : " (not an attack)";
        string line = $"Hit (turn {turn}): {from}, {hit.Amount:0} damage{kind}";

        if (hit.Blocker != null)
        {
            line += hit.BlockerResult is { } taken
                ? $"; {Name(hit.Blocker)} took {taken.UnblockedDamage} of its {hit.BlockerHp} HP{(taken.WasTargetKilled ? ", died" : "")}"
                : $"; {Name(hit.Blocker)} was to block but took nothing";
        }
        else if (target == hit.Player.Creature && hit.Dealer is { IsEnemy: true } && hit.Props.IsPoweredAttack())
        {
            line += Board.CreatureInLane(hit.Player, hit.Lane) is { } occupant
                ? $"; {Name(occupant)} in that lane could not block"
                : hit.Lane >= 0 ? "; lane empty" : "; attacker has no lane";
        }

        var blockHolder = target.PetOwner?.Creature ?? target;
        line += $"; {Name(target)}: Block {hit.Block} -> {blockHolder.Block}, HP {hit.Hp} -> {target.CurrentHp}";
        if (target != hit.Player.Creature && result.WasTargetKilled)
        {
            line += ", died";
        }
        return line;
    }

    private static string Name(Creature creature) => creature.IsPlayer ? "Luke" : creature.Monster?.Id.Entry ?? "?";
}
