using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The free creature: two in the starting deck, and one each turn from <see cref="Relics.SideDeck"/>; exhausts.
/// </summary>
public sealed class Squirrel() : CreatureCard<SquirrelCreature>(Bestiary.Squirrel, CardRarity.Token)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [.. base.CanonicalKeywords, CardKeyword.Exhaust];
}
