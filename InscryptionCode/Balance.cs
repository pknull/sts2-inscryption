using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode;

/// <summary>
/// Numbers that translate Inscryption's scale into Slay the Spire 2's. Provisional: tune after playtesting.
/// </summary>
public static class Balance
{
    /// <summary>Inscryption power is multiplied by this (a 1-power Stoat hits for 2).</summary>
    public const int PowerScale = 2;

    /// <summary>
    /// Inscryption health is multiplied by this (a 3-health Stoat has 6 HP). Lowered from x5 with the energy costs
    /// (Keeper, 2026-10-06, #15): at x5 one large blocker held a lane for the whole fight.
    /// </summary>
    public const int HealthScale = 2;

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
