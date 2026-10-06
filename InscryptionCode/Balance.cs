namespace Inscryption.InscryptionCode;

/// <summary>
/// Numbers that translate Inscryption's scale into Slay the Spire 2's. Provisional: tune after playtesting.
/// </summary>
public static class Balance
{
    /// <summary>Inscryption power is multiplied by this (a 1-power Stoat hits for 3).</summary>
    public const int PowerScale = 3;

    /// <summary>
    /// Inscryption health is multiplied by this (a 3-health Stoat has 15 HP). Higher than PowerScale because
    /// Slay the Spire 2's enemies hit harder than Inscryption's; at x3 a Stoat died to one 12-damage hit.
    /// </summary>
    public const int HealthScale = 5;

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
