namespace Inscryption.InscryptionCode.Creatures;

/// <summary>Inscryption's printed stats for a creature card, before <see cref="Balance.StatScale"/>.</summary>
public sealed record CreatureStats(int Blood, int Power, int Health);

/// <summary>
/// Act 1 stats, from the Inscryption wiki (scratch reference: inscryption-reference.md). Both the card and the
/// creature it summons read from here, so the numbers live in one place.
/// </summary>
public static class Bestiary
{
    public static readonly CreatureStats Squirrel = new(Blood: 0, Power: 0, Health: 1);
    public static readonly CreatureStats Stoat = new(Blood: 1, Power: 1, Health: 3);
    public static readonly CreatureStats Bullfrog = new(Blood: 1, Power: 1, Health: 2);
    public static readonly CreatureStats Wolf = new(Blood: 2, Power: 3, Health: 2);
}
