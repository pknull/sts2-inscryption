using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class CorpseMaggotsCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.CorpseMaggots;
    public override CardModel Card => ModelDb.Card<CorpseMaggots>();
}
