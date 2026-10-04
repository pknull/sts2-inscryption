using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>The side deck's free creature. Generated each turn by <see cref="Relics.SideDeck"/>; exhausts.</summary>
public sealed class Squirrel() : CreatureCard<SquirrelCreature>(Bestiary.Squirrel, CardRarity.Token)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}
