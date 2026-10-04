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

    /// <summary>Squirrels in the side deck per combat.</summary>
    public const int SideDeckSize = 10;
}
