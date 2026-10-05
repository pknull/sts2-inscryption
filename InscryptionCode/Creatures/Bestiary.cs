namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption's printed stats for a creature card, before <see cref="Balance.PowerScale"/> and
/// <see cref="Balance.HealthScale"/>. A card costs Blood or Bones (or nothing), as in Inscryption.
/// </summary>
public sealed record CreatureStats(
    int Blood, int Power, int Health, int Bones = 0, Tribe Tribe = Tribe.None, Sigil[]? SigilList = null)
{
    public bool IsFree => Blood == 0 && Bones == 0;

    public IReadOnlyList<Sigil> Sigils => SigilList ?? [];
}

/// <summary>
/// Act 1 stats, from the Inscryption wiki (scratch reference: inscryption-reference.md). Both the card and the
/// creature it summons read from here, so the numbers live in one place.
/// </summary>
public static class Bestiary
{
    public static readonly CreatureStats Squirrel = new(Blood: 0, Power: 0, Health: 1, Tribe: Tribe.Squirrel);
    public static readonly CreatureStats Stoat = new(Blood: 1, Power: 1, Health: 3);
    public static readonly CreatureStats Bullfrog = new(Blood: 1, Power: 1, Health: 2, Tribe: Tribe.Reptile,
        SigilList: [Sigil.MightyLeap]);
    public static readonly CreatureStats Wolf = new(Blood: 2, Power: 3, Health: 2, Tribe: Tribe.Canine);
    public static readonly CreatureStats Grizzly = new(Blood: 3, Power: 4, Health: 6);
    public static readonly CreatureStats RiverSnapper = new(Blood: 2, Power: 1, Health: 6, Tribe: Tribe.Reptile);
    public static readonly CreatureStats RingWorm = new(Blood: 1, Power: 0, Health: 1, Tribe: Tribe.Insect);
    public static readonly CreatureStats Urayuli = new(Blood: 4, Power: 7, Health: 7);
    public static readonly CreatureStats Amalgam = new(Blood: 2, Power: 3, Health: 3, Tribe: Tribe.All);
    public static readonly CreatureStats Geck = new(Blood: 0, Power: 1, Health: 1, Tribe: Tribe.Reptile);
    public static readonly CreatureStats Opossum = new(Blood: 0, Power: 1, Health: 1, Bones: 2);
    public static readonly CreatureStats Coyote = new(Blood: 0, Power: 2, Health: 1, Bones: 4, Tribe: Tribe.Canine);
    public static readonly CreatureStats Rattler = new(Blood: 0, Power: 3, Health: 1, Bones: 6, Tribe: Tribe.Reptile);

    // Batch 1: lane and combat sigils.
    public static readonly CreatureStats Sparrow = new(Blood: 1, Power: 1, Health: 2, Tribe: Tribe.Avian,
        SigilList: [Sigil.Airborne]);
    public static readonly CreatureStats Raven = new(Blood: 2, Power: 2, Health: 3, Tribe: Tribe.Avian,
        SigilList: [Sigil.Airborne]);
    public static readonly CreatureStats Bat = new(Blood: 0, Power: 2, Health: 1, Bones: 4,
        SigilList: [Sigil.Airborne]);
    public static readonly CreatureStats TurkeyVulture = new(Blood: 0, Power: 3, Health: 3, Bones: 8, Tribe: Tribe.Avian,
        SigilList: [Sigil.Airborne]);
    public static readonly CreatureStats Kingfisher = new(Blood: 1, Power: 1, Health: 1, Tribe: Tribe.Avian,
        SigilList: [Sigil.Airborne, Sigil.Waterborne]);
    public static readonly CreatureStats Mantis = new(Blood: 1, Power: 1, Health: 1, Tribe: Tribe.Insect,
        SigilList: [Sigil.BifurcatedStrike]);
    public static readonly CreatureStats MantisGod = new(Blood: 1, Power: 1, Health: 1, Tribe: Tribe.Insect,
        SigilList: [Sigil.TrifurcatedStrike]);
    public static readonly CreatureStats Pronghorn = new(Blood: 2, Power: 1, Health: 3, Tribe: Tribe.Hooved,
        SigilList: [Sigil.Sprinter, Sigil.BifurcatedStrike]);
    public static readonly CreatureStats Elk = new(Blood: 2, Power: 2, Health: 4, Tribe: Tribe.Hooved,
        SigilList: [Sigil.Sprinter]);
    public static readonly CreatureStats LongElk = new(Blood: 0, Power: 1, Health: 2, Bones: 4, Tribe: Tribe.Hooved,
        SigilList: [Sigil.Sprinter, Sigil.TouchOfDeath]);
    public static readonly CreatureStats Alpha = new(Blood: 0, Power: 1, Health: 2, Bones: 4, Tribe: Tribe.Canine,
        SigilList: [Sigil.Leader]);
    public static readonly CreatureStats Bloodhound = new(Blood: 2, Power: 2, Health: 3, Tribe: Tribe.Canine,
        SigilList: [Sigil.Guardian]);
    public static readonly CreatureStats Skunk = new(Blood: 1, Power: 0, Health: 3, SigilList: [Sigil.Stinky]);
    public static readonly CreatureStats Porcupine = new(Blood: 1, Power: 1, Health: 2, SigilList: [Sigil.SharpQuills]);
    public static readonly CreatureStats Adder = new(Blood: 2, Power: 1, Health: 1, Tribe: Tribe.Reptile,
        SigilList: [Sigil.TouchOfDeath]);
    public static readonly CreatureStats GreatWhite = new(Blood: 3, Power: 4, Health: 2, SigilList: [Sigil.Waterborne]);
    public static readonly CreatureStats RiverOtter = new(Blood: 1, Power: 1, Health: 1, SigilList: [Sigil.Waterborne]);
    public static readonly CreatureStats Mole = new(Blood: 1, Power: 0, Health: 4, SigilList: [Sigil.Burrower]);
    public static readonly CreatureStats MoleMan = new(Blood: 1, Power: 0, Health: 6,
        SigilList: [Sigil.Burrower, Sigil.MightyLeap]);
}
