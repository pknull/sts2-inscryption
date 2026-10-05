using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>Inscryption's sigils, as far as they are implemented. Behaviour lives in <see cref="Powers.CreaturePower"/>.</summary>
public enum Sigil
{
    None,
    Airborne,
    MightyLeap,
    BifurcatedStrike,
    TrifurcatedStrike,
    Leader,
    Stinky,
    SharpQuills,
    TouchOfDeath,
    Waterborne,
    Burrower,
    Guardian,
    Sprinter,
}

/// <summary>Inscryption's tribes; Totems grant a sigil to every creature of one tribe.</summary>
[Flags]
public enum Tribe
{
    None = 0,
    Canine = 1,
    Hooved = 2,
    Reptile = 4,
    Avian = 8,
    Insect = 16,
    Squirrel = 32,
    All = Canine | Hooved | Reptile | Avian | Insect | Squirrel,
}

public static class Sigils
{
    /// <summary>Sigils a Totem can carry (Inscryption's "modular" sigils; all implemented ones qualify).</summary>
    public static readonly Sigil[] Modular = Enum.GetValues<Sigil>().Where(s => s != Sigil.None).ToArray();

    public static readonly Tribe[] Tribes = [Tribe.Canine, Tribe.Hooved, Tribe.Reptile, Tribe.Avian, Tribe.Insect, Tribe.Squirrel];

    /// <summary>
    /// Slay the Spire monsters that fly. Their hits pass over blockers to the player unless the blocker has Mighty
    /// Leap. Found in the decompile by their flight animations and moves; extend as more are spotted in game.
    /// </summary>
    private static readonly HashSet<Type> Flyers = [typeof(Byrdonis), typeof(OwlMagistrate), typeof(ThievingHopper)];

    public static bool IsAirborneEnemy(Creature? enemy) => enemy?.Monster != null && Flyers.Contains(enemy.Monster.GetType());

    // The keyword fields are filled in by BaseLib at runtime, so map lazily rather than caching them in static data.
    public static CardKeyword Keyword(Sigil sigil) => sigil switch
    {
        Sigil.Airborne => InscryptionKeywords.Airborne,
        Sigil.MightyLeap => InscryptionKeywords.MightyLeap,
        Sigil.BifurcatedStrike => InscryptionKeywords.BifurcatedStrike,
        Sigil.TrifurcatedStrike => InscryptionKeywords.TrifurcatedStrike,
        Sigil.Leader => InscryptionKeywords.Leader,
        Sigil.Stinky => InscryptionKeywords.Stinky,
        Sigil.SharpQuills => InscryptionKeywords.SharpQuills,
        Sigil.TouchOfDeath => InscryptionKeywords.TouchOfDeath,
        Sigil.Waterborne => InscryptionKeywords.Waterborne,
        Sigil.Burrower => InscryptionKeywords.Burrower,
        Sigil.Guardian => InscryptionKeywords.Guardian,
        Sigil.Sprinter => InscryptionKeywords.Sprinter,
        _ => throw new ArgumentOutOfRangeException(nameof(sigil)),
    };

    public static CardKeyword Keyword(Tribe tribe) => tribe switch
    {
        Tribe.Canine => InscryptionKeywords.CanineTribe,
        Tribe.Hooved => InscryptionKeywords.HoovedTribe,
        Tribe.Reptile => InscryptionKeywords.ReptileTribe,
        Tribe.Avian => InscryptionKeywords.AvianTribe,
        Tribe.Insect => InscryptionKeywords.InsectTribe,
        Tribe.Squirrel => InscryptionKeywords.SquirrelTribe,
        Tribe.All => InscryptionKeywords.EveryTribe,
        _ => throw new ArgumentOutOfRangeException(nameof(tribe)),
    };

    /// <summary>Keywords shown on a creature card: its tribe (if any), then its sigils.</summary>
    public static IEnumerable<CardKeyword> Keywords(CreatureStats stats)
    {
        if (stats.Tribe != Tribe.None)
        {
            yield return Keyword(stats.Tribe);
        }
        foreach (var sigil in stats.Sigils)
        {
            yield return Keyword(sigil);
        }
    }

    // Totems played this combat, per combat (a new combat state starts with none).
    private static readonly ConditionalWeakTable<ICombatState, List<(Tribe Tribe, Sigil Sigil)>> Totems = new();

    public static void AddTotem(ICombatState combatState, Tribe tribe, Sigil sigil) =>
        Totems.GetOrCreateValue(combatState).Add((tribe, sigil));

    /// <summary>Does this creature of ours have the sigil, from its card or from a Totem of its tribe?</summary>
    public static bool Has(Creature creature, Sigil sigil)
    {
        if (creature.Monster is not BoardCreature board)
        {
            return false;
        }
        if (board.OwnSigils.Contains(sigil))
        {
            return true;
        }
        return creature.CombatState != null && Totems.TryGetValue(creature.CombatState, out var totems)
            && totems.Any(t => t.Sigil == sigil && (board.Stats.Tribe & t.Tribe) != 0);
    }

    public static IEnumerable<Sigil> All(Creature creature) => Modular.Where(s => Has(creature, s));

    /// <summary>The lanes a creature strikes: its own, or its neighbours (Bifurcated), or all three (Trifurcated).</summary>
    public static IEnumerable<int> StrikeLanes(Creature creature)
    {
        int lane = Board.LaneOf(creature);
        int[] offsets = Has(creature, Sigil.TrifurcatedStrike) ? [-1, 0, 1]
            : Has(creature, Sigil.BifurcatedStrike) ? [-1, 1] : [0];
        return lane < 0 ? [] : offsets.Select(o => lane + o).Where(l => l >= 0 && l < Board.LaneCount);
    }
}
