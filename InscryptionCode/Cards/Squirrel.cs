using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>The free creature, sacrifice fodder: two in the starting deck, more from rewards and the shop.</summary>
public sealed class Squirrel() : CreatureCard<SquirrelCreature>(Bestiary.Squirrel, CardRarity.Common);
