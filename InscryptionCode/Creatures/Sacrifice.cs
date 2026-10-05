using System.Runtime.CompilerServices;
using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Paying Blood. Each creature is worth 1 Blood (3 with Worthy Sacrifice; terrain is worth nothing and cannot be
/// chosen). A sacrifice dies, unless it has Many Lives: then it pays and stays. The summon still needs an empty lane,
/// so a choice that would leave none is never offered.
/// </summary>
public static class Sacrifice
{
    // Creatures killed as a sacrifice, so death sigils that only fire "in combat" (Corpse Eater, Frozen Away) skip them.
    private static readonly ConditionalWeakTable<Creature, object> Sacrificed = new();

    public static bool WasSacrificed(Creature creature) => Sacrificed.TryGetValue(creature, out _);

    public static int Value(Creature creature)
    {
        if (Sigils.Has(creature, Sigil.WorthySacrifice))
        {
            return 3;
        }
        return creature.Monster is BoardCreature { Stats.Terrain: true } ? 0 : 1;
    }

    /// <summary>Many Lives: pays its Blood without dying (a Cat that has spent all nine lives dies).</summary>
    public static bool Stays(Creature creature) =>
        Sigils.Has(creature, Sigil.ManyLives) && creature.Monster is not BoardCreature { SourceCard: Cat { LivesLeft: 0 } };

    /// <summary>The player's creatures that can be sacrificed, in lane order.</summary>
    public static List<Creature> Candidates(Player owner) => Board.Creatures(owner).Where(c => Value(c) > 0).ToList();

    public static int EmptyLanes(Player owner) => Board.LaneCount - Board.Creatures(owner).Count;

    /// <summary>Can the player pay <paramref name="blood"/> and still have a lane for the summon?</summary>
    public static bool CanPay(Player owner, int blood)
    {
        int empty = EmptyLanes(owner);
        if (blood <= 0)
        {
            return empty > 0;
        }
        var options = Candidates(owner);
        return options.Sum(Value) >= blood && empty + options.Count(c => !Stays(c)) > 0;
    }

    /// <summary>
    /// After picking <paramref name="next"/> on top of <paramref name="picked"/>, can the payment still be completed
    /// with a lane left over for the summon?
    /// </summary>
    public static bool Feasible(IReadOnlyCollection<Creature> picked, Creature next, IReadOnlyList<Creature> options,
        int blood, int emptyLanes)
    {
        var after = picked.Append(next).ToList();
        int paid = after.Sum(Value);
        int freed = after.Count(c => !Stays(c));
        if (paid >= blood)
        {
            return emptyLanes + freed > 0;
        }
        var rest = options.Except(after).ToList();
        return paid + rest.Sum(Value) >= blood && emptyLanes + freed + rest.Count(c => !Stays(c)) > 0;
    }

    /// <summary>Kill the chosen creatures, except those with Many Lives (the Cat counts the life it spent).</summary>
    public static async Task Perform(IEnumerable<Creature> victims)
    {
        foreach (var victim in victims)
        {
            if (Stays(victim))
            {
                // The Cat's lives: shown on this combat's card, kept (and spent for good) on the deck's.
                if (victim.Monster is BoardCreature { SourceCard: Cat cat })
                {
                    await cat.LoseLife();
                    if (cat.DeckVersion is Cat deckCat)
                    {
                        await deckCat.LoseLife();
                    }
                }
                continue;
            }
            Sacrificed.AddOrUpdate(victim, true);
            await CreatureCmd.Kill(victim);
        }
    }
}
