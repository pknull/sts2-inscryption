namespace Inscryption.InscryptionCode;

/// <summary>
/// Numbers that translate Inscryption's scale into Slay the Spire 2's. Provisional: tune after playtesting.
/// </summary>
public static class Balance
{
    /// <summary>Inscryption power and health are multiplied by this (a 1/3 Stoat becomes 3 damage, 9 HP).</summary>
    public const int StatScale = 3;

    /// <summary>Inscryption's board has four lanes.</summary>
    public const int MaxCreatures = 4;

    /// <summary>Squirrels in the side deck per combat.</summary>
    public const int SideDeckSize = 10;
}
