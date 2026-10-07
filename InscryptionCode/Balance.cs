using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode;

/// <summary>
/// Numbers that translate Inscryption's scale into Slay the Spire 2's. Provisional: tune after playtesting.
/// </summary>
public static class Balance
{
    // Creatures are priced like cards (Keeper, 2026-10-07): a Blood (sacrifice) is worth an energy, and each energy of
    // a creature's cost buys a full Attack's worth AND a full Block's worth, as vanilla's lasting effects do (Zap: a
    // Lightning orb dealing 3 a turn, plus its evoke; Glacier: Block plus Frost). One energy of a lasting strike is the
    // Lightning orb's 3 a turn; one energy of HP is a card's 6 Block. Inscryption's Commons average 1 Power and 1.5
    // Health per Blood, hence x3 and x4; Inscryption's own stat lines keep each creature's shape and sigil discount.

    /// <summary>Inscryption power is multiplied by this (a 1-power Stoat hits for 3).</summary>
    public const int PowerScale = 3;

    /// <summary>Inscryption health is multiplied by this (a 3-health Stoat has 12 HP).</summary>
    public const int HealthScale = 4;

    /// <summary>Damage per turn that each energy of a creature card's cost buys, spread over the lanes it strikes.</summary>
    public const int DamagePerEnergy = 3;

    /// <summary>HP that each energy of a creature card's cost buys.</summary>
    public const int HealthPerEnergy = 6;

    /// <summary>
    /// A creature's damage per strike for <paramref name="power"/> Inscryption Power, with the energy its card costs
    /// (<paramref name="energy"/>). A creature with no power doesn't strike, so the energy buys it none.
    /// </summary>
    public static int Damage(int power, CreatureStats stats, int energy) =>
        power <= 0 ? 0 : power * PowerScale + (int)Math.Round((decimal)DamagePerEnergy * energy / LanesStruck(stats),
            MidpointRounding.AwayFromZero);

    /// <summary>A creature's HP for <paramref name="health"/> Inscryption Health, with its card's energy cost.</summary>
    public static int Health(int health, int energy) => health * HealthScale + HealthPerEnergy * energy;

    private static int LanesStruck(CreatureStats stats) =>
        stats.Sigils.Contains(Sigil.TrifurcatedStrike) ? 3 : stats.Sigils.Contains(Sigil.BifurcatedStrike) ? 2 : 1;

    /// <summary>
    /// A creature card's energy cost, on top of its Blood or Bones (Keeper, 2026-10-06, #15): Commons and tokens are
    /// free, Uncommons cost 1 and Rares 2.
    /// </summary>
    public static int EnergyCost(CardRarity rarity) => rarity switch
    {
        CardRarity.Uncommon => 1,
        CardRarity.Rare => 2,
        _ => 0,
    };

    /// <summary>Squirrels in the side deck per combat (Inscryption's side deck holds ten).</summary>
    public const int SideDeckSize = 10;

    /// <summary>Inscryption's board has four lanes.</summary>
    public const int MaxCreatures = 4;


    /// <summary>
    /// Does a blocker soak the whole hit (Inscryption), or only up to its HP with the rest reaching the player (the
    /// engine's own rule, as Osty's overflow reaches the Necrobinder)? Whole-hit soaking let a free Squirrel cancel any
    /// hit; off for testing (Keeper, 2026-10-05).
    /// </summary>
    public const bool BlockersSoakOverkill = false;

    /// <summary>Inscryption's campfire warms a creature for +1 Power or +2 Health (before scaling).</summary>
    public const int CampfirePower = 1;

    public const int CampfireHealth = 2;
}
