namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption's printed stats for a creature card, before <see cref="Balance.PowerScale"/> and
/// <see cref="Balance.HealthScale"/>. A card costs Blood or Bones (or nothing), as in Inscryption.
/// </summary>
public sealed record CreatureStats(int Blood, int Power, int Health, int Bones = 0)
{
    public bool IsFree => Blood == 0 && Bones == 0;
}

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
    public static readonly CreatureStats Grizzly = new(Blood: 3, Power: 4, Health: 6);
    public static readonly CreatureStats RiverSnapper = new(Blood: 2, Power: 1, Health: 6);
    public static readonly CreatureStats RingWorm = new(Blood: 1, Power: 0, Health: 1);
    public static readonly CreatureStats Urayuli = new(Blood: 4, Power: 7, Health: 7);
    public static readonly CreatureStats Amalgam = new(Blood: 2, Power: 3, Health: 3);
    public static readonly CreatureStats Geck = new(Blood: 0, Power: 1, Health: 1);
    public static readonly CreatureStats Opossum = new(Blood: 0, Power: 1, Health: 1, Bones: 2);
    public static readonly CreatureStats Coyote = new(Blood: 0, Power: 2, Health: 1, Bones: 4);
    public static readonly CreatureStats Rattler = new(Blood: 0, Power: 3, Health: 1, Bones: 6);
}
