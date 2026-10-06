using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>Inscryption's first creature; a card reward (the starting deck carries Squirrels instead).</summary>
public sealed class Stoat() : CreatureCard<StoatCreature>(Bestiary.Stoat, CardRarity.Common);
