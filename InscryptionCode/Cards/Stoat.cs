using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>The starting creature: two in the starting deck, beside the Strikes and Defends.</summary>
public sealed class Stoat() : CreatureCard<StoatCreature>(Bestiary.Stoat, CardRarity.Basic);
