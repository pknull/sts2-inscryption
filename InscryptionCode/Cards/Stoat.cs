using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>The starter wall (a Skill); tagged Defend because the game expects one (Neow's Large Capsule, Fasten).</summary>
public sealed class Stoat() : CreatureCard<StoatCreature>(Bestiary.Stoat, CardRarity.Basic)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
}
