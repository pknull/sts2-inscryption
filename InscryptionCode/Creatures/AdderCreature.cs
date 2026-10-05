using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class AdderCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Adder;
    public override CardModel Card => ModelDb.Card<Adder>();
}
