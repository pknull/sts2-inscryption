using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>The starter attacker (an Attack); tagged Strike because the game expects one (Neow's Large Capsule).</summary>
public sealed class Wolf() : CreatureCard<WolfCreature>(Bestiary.Wolf, CardRarity.Basic)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
}
