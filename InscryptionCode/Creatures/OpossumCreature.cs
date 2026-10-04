using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class OpossumCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Opossum;
    public override CardModel Card => ModelDb.Card<Opossum>();
}
