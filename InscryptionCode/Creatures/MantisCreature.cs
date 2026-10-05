using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class MantisCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Mantis;
    public override CardModel Card => ModelDb.Card<Mantis>();
}
